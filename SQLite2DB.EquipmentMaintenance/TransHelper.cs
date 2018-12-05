using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Data.SQLite;
using System.Collections.Generic;
using Ionic.Zip;
using Utility;
using Utility.Models;
#if ORACLE
using DbEntity.ORACLE;
using DbEntity.ORACLE.EquipmentMaintenance;
#else
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
#endif
using DataAccess;
using System.Net.Mail;
using System.Text;
using SQLite2DB.EquipmentMaintenance.Models;
using DataAccess.EquipmentMaintenance;
using Models.EquipmentMaintenance.ResultQuery;

namespace SQLite2DB.EquipmentMaintenance
{
    public class TransHelper : IDisposable
    {
        public void Trans()
        {
            try
            {
                var folders = Directory.GetDirectories(Config.EquipmentMaintenanceSQLiteUploadFolderPath);

                //從Upload資料夾搬移到Processing資料夾
                foreach (var folder in folders)
                {
                    System.IO.File.Move(Path.Combine(folder, "EquipmentMaintenance.Upload.zip"), Path.Combine(Config.EquipmentMaintenanceSQLiteProcessingFolderPath, folder.Substring(folder.LastIndexOf("\\") + 1) + ".zip"));

                    Directory.Delete(folder);
                }

                var zips = Directory.GetFiles(Config.EquipmentMaintenanceSQLiteProcessingFolderPath);

                Logger.Log(zips.Length + " Files To Trans");

                foreach (var zip in zips)
                {
                    FileInfo fileInfo = new FileInfo(zip);

                    var uploadLogUniqueID = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(fileInfo.Extension));
                   
                    RequestResult result = Trans(uploadLogUniqueID, fileInfo);

                    //轉檔成功->搬移到Backup資料夾 & Update Upload Log
                    if (result.IsSuccess)
                    {
                        using (EDbEntities db = new EDbEntities())
                        {
                            var uploadLog = db.UploadLog.FirstOrDefault(x => x.UniqueID == uploadLogUniqueID);

                            if (uploadLog != null)
                            {
                                uploadLog.TransTime = DateTime.Now;

                                db.SaveChanges();
                            }
                        }

                        System.IO.File.Copy(zip, Path.Combine(Config.EquipmentMaintenanceSQLiteBackupFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.EquipmentMaintenanceSQLiteProcessingFolderPath, uploadLogUniqueID), true);

                        Logger.Log(zip + " Trans Success");
                    }
                    //轉檔失敗->搬移到Error資料夾
                    else
                    {
                        System.IO.File.Copy(zip, Path.Combine(Config.EquipmentMaintenanceSQLiteErrorFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.EquipmentMaintenanceSQLiteProcessingFolderPath, uploadLogUniqueID), true);

                        Logger.Log(zip + " Trans Failed");
                    }
                       
                }
            }
            catch (Exception ex)
            {
                Logger.Log(new Error(MethodBase.GetCurrentMethod(), ex));
            }
        }

        private RequestResult Trans(string UploadLogUniqueID, FileInfo ZipFileInfo)
        {
            RequestResult result = new RequestResult();

            try
            {
                var extractPath = Path.Combine(Config.EquipmentMaintenanceSQLiteProcessingFolderPath, UploadLogUniqueID);

                using (var zip = ZipFile.Read(ZipFileInfo.FullName))
                {
                    foreach (var entry in zip)
                    {
                        entry.Extract(extractPath);
                    }
                }

                var connString = string.Format("Data Source={0};Version=3;", Path.Combine(extractPath, Define.SQLite_EquipmentMaintenance));

                using (SQLiteConnection conn = new SQLiteConnection(connString))
                {
                    conn.Open();

                    var uploadList = new List<string>();

                    #region UploadDefine
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM UploadDefine";

                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                adapter.Fill(dt);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    uploadList = dt.AsEnumerable().Select(x => x["JobUniqueID"].ToString()).ToList();
                                }
                            }
                        }
                    }
                    #endregion

                    #region JobResult
                    var jobResultList = new List<JobResultModel>();

                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT JobUniqueID, ArriveDate, ArriveTime FROM ArriveRecord";

                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                adapter.Fill(dt);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    var temp = dt.AsEnumerable().Select(x => new
                                    {
                                        JobUniqueID = x["JobUniqueID"].ToString(),
                                        CheckDate = x["ArriveDate"].ToString(),
                                        CheckTime = x["ArriveTime"].ToString()
                                    }).Distinct().ToList();

                                    var jobUniqueIDList = temp.Where(x => uploadList.Contains(x.JobUniqueID)).Select(x => x.JobUniqueID).Distinct().ToList();

                                    using (EDbEntities edb = new EDbEntities())
                                    {
                                        foreach (var jobUniqueID in jobUniqueIDList)
                                        {
                                            var job = edb.Job.FirstOrDefault(x => x.UniqueID == jobUniqueID);

                                            if (job != null)
                                            {
#if CHIMEI
                                                DateTime jobBeginTime, jobEndTime;
                                                
                                                JobCycleHelper.GetDateSpan(job.BeginDate, job.BeginDate, job.EndDate, job.CycleCount, job.CycleMode, out jobBeginTime, out jobEndTime);

                                                var jobBeginDateString = DateTimeHelper.DateTime2DateString(jobBeginTime);
                                                var jobEndDateString = DateTimeHelper.DateTime2DateString(jobEndTime);

                                                var jobResult = edb.JobResult.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.BeginDate == jobBeginDateString && x.EndDate == jobEndDateString);

                                                jobResultList.Add(new JobResultModel()
                                                {
                                                    UniqueID = jobResult != null ? jobResult.UniqueID : Guid.NewGuid().ToString(),
                                                    JobUniqueID = job.UniqueID,
                                                    BeginDate = jobBeginDateString,
                                                    EndDate = jobEndDateString,
                                                    CurrentOverTimeReason = jobResult != null ? jobResult.OverTimeReason : string.Empty,
                                                    CurrentOverTimeUser = jobResult != null ? jobResult.OverTimeUser : string.Empty,
                                                    CurrentUnPatrolReason = jobResult != null ? jobResult.UnPatrolReason : string.Empty,
                                                    CurrentUnPatrolUser = jobResult != null ? jobResult.UnPatrolUser : string.Empty
                                                });
#else
                                                //用同一派工的最早到位日期當作派工周期的判斷基準
                                                var firstArriveDateTime = temp.Where(x => x.JobUniqueID == jobUniqueID && !string.IsNullOrEmpty(x.CheckDate) && !string.IsNullOrEmpty(x.CheckTime)).OrderBy(x => x.CheckDate).ThenBy(x => x.CheckTime).Select(x => new { x.CheckDate, x.CheckTime }).ToList().First();

                                                DateTime jobBeginTime, jobEndTime;
                                                string jobBeginDateString, jobEndDateString;

                                                JobCycleHelper.GetDateTimeSpan(firstArriveDateTime.CheckDate, firstArriveDateTime.CheckTime, job.BeginDate, job.BeginTime, job.EndDate, job.EndTime, job.CycleCount, job.CycleMode, out jobBeginTime, out jobBeginDateString, out jobEndTime, out jobEndDateString);

                                                var jobResult = edb.JobResult.FirstOrDefault(x => x.JobUniqueID == job.UniqueID && x.BeginDate == jobBeginDateString && x.EndDate == jobEndDateString);

                                                jobResultList.Add(new JobResultModel()
                                                {
                                                    UniqueID = jobResult != null ? jobResult.UniqueID : Guid.NewGuid().ToString(),
                                                    JobUniqueID = job.UniqueID,
                                                    BeginDate = jobBeginDateString,
                                                    EndDate = jobEndDateString,
                                                    CurrentOverTimeReason = jobResult != null ? jobResult.OverTimeReason : string.Empty,
                                                    CurrentOverTimeUser = jobResult != null ? jobResult.OverTimeUser : string.Empty,
                                                    CurrentUnPatrolReason = jobResult != null ? jobResult.UnPatrolReason : string.Empty,
                                                    CurrentUnPatrolUser = jobResult != null ? jobResult.UnPatrolUser : string.Empty
                                                });
#endif
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region ArriveRecord
                    using (EDbEntities db = new EDbEntities())
                    {
                        foreach (var jobResult in jobResultList)
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = string.Format("SELECT * FROM ArriveRecord WHERE JobUniqueID = '{0}'", jobResult.JobUniqueID);
                                

                                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                                {
                                    using (DataTable dt = new DataTable())
                                    {
                                        adapter.Fill(dt);

                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            var uniqueID = dt.Rows[i]["UniqueID"].ToString();

                                            if (!db.ArriveRecord.Any(x => x.UniqueID == uniqueID))
                                            {
                                                var jobUniqueID = dt.Rows[i]["JobUniqueID"].ToString();

                                                var controlPointUniqueID = dt.Rows[i]["ControlPointUniqueID"].ToString();

                                                var query = (from x in db.JobControlPoint
                                                             join c in db.ControlPoint
                                                             on x.ControlPointUniqueID equals c.UniqueID
                                                             join j in db.Job
                                                             on x.JobUniqueID equals j.UniqueID
                                                             join r in db.Route
                                                             on j.RouteUniqueID equals r.UniqueID
                                                             where x.JobUniqueID == jobUniqueID && x.ControlPointUniqueID == controlPointUniqueID
                                                             select new
                                                             {
                                                                 r.OrganizationUniqueID,
                                                                 RouteUniqueID = r.UniqueID,
                                                                 RouteID = r.ID,
                                                                 RouteName = r.Name,
                                                                 JobUniqueID = j.UniqueID,
                                                                 JobDescription = j.Description,
                                                                 ControlPointUniqueID = c.UniqueID,
                                                                 ControlPointID = c.ID,
                                                                 ControlPointDescription = c.Description,
                                                                 j.CycleCount,
                                                                 j.CycleMode,
                                                                 j.BeginDate,
                                                                 j.EndDate
                                                             }).FirstOrDefault();

                                                if (query != null)
                                                {
                                                    var unRFIDReasonUniqueID = dt.Rows[i]["UnRFIDReasonUniqueID"].ToString();
                                                    var unRFIDReason = db.UnRFIDReason.FirstOrDefault(x => x.UniqueID == unRFIDReasonUniqueID);

                                                    var timeSpanAbnormalReasonUniqueID = string.Empty;
                                                    var timeSpanAbnormalReasonRemark = string.Empty;
                                                    var totalTimeSpan = string.Empty;
                                                    var minTimeSpan = string.Empty;

                                                    try
                                                    {
                                                        timeSpanAbnormalReasonUniqueID = dt.Rows[i]["TimeSpanAbnormalReasonUniqueID"].ToString();
                                                        timeSpanAbnormalReasonRemark = dt.Rows[i]["TimeSpanAbnormalReasonRemark"].ToString();

                                                        totalTimeSpan = dt.Rows[i]["TotalTimeSpan"].ToString();
                                                        minTimeSpan = dt.Rows[i]["MinTimeSpan"].ToString();
                                                    }
                                                    catch { }

                                                    var timeSpanAbnormalReason = db.TimeSpanAbnormalReason.FirstOrDefault(x => x.UniqueID == timeSpanAbnormalReasonUniqueID);

                                                    var userID = dt.Rows[i]["UserID"].ToString();

                                                    var user = UserDataAccessor.GetUser(userID);

                                                    db.ArriveRecord.Add(new ArriveRecord()
                                                    {
                                                        UniqueID = uniqueID,
                                                        JobResultUniqueID = jobResult.UniqueID,
                                                        OrganizationUniqueID = query.OrganizationUniqueID,
                                                        JobUniqueID = query.JobUniqueID,
                                                        JobDescription = query.JobDescription,
                                                        RouteUniqueID = query.RouteUniqueID,
                                                        RouteID = query.RouteID,
                                                        RouteName = query.RouteName,
                                                        ControlPointUniqueID = query.ControlPointUniqueID,
                                                        ControlPointID = query.ControlPointID,
                                                        ControlPointDescription = query.ControlPointDescription,
                                                        ArriveDate = dt.Rows[i]["ArriveDate"].ToString(),
                                                        ArriveTime = dt.Rows[i]["ArriveTime"].ToString(),
                                                        UserID = userID,
                                                        UserName = user != null ? user.Name : "",
                                                        UnRFIDReasonUniqueID = unRFIDReasonUniqueID,
                                                        UnRFIDReasonID = unRFIDReason != null ? unRFIDReason.ID : (unRFIDReasonUniqueID == "OTHER" ? "OTHER" : ""),
                                                        UnRFIDReasonDescription = unRFIDReason != null ? unRFIDReason.Description : (unRFIDReasonUniqueID == "OTHER" ? Resources.Resource.Other : ""),
                                                        UnRFIDReasonRemark = dt.Rows[i]["UnRFIDReasonRemark"].ToString(),
                                                        MinTimeSpan = !string.IsNullOrEmpty(minTimeSpan) ? double.Parse(minTimeSpan) : default(double?),
                                                        TotalTimeSpan = !string.IsNullOrEmpty(totalTimeSpan) ? double.Parse(totalTimeSpan) : default(double?),
                                                        TimeSpanAbnormalReasonUniqueID = timeSpanAbnormalReasonUniqueID,
                                                        TimeSpanAbnormalReasonID = timeSpanAbnormalReason != null ? timeSpanAbnormalReason.ID : (timeSpanAbnormalReasonUniqueID == "OTHER" ? "OTHER" : ""),
                                                        TimeSpanAbnormalReasonDescription = timeSpanAbnormalReason != null ? timeSpanAbnormalReason.Description : (timeSpanAbnormalReasonUniqueID == "OTHER" ? Resources.Resource.Other : ""),
                                                        TimeSpanAbnormalReasonRemark = timeSpanAbnormalReasonRemark
                                                    });

                                                    TransArriveRecordPhoto(db, conn, uniqueID, extractPath);
                                                }
                                            }
                                        }

                                        db.SaveChanges();
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    var abnormalList = new Dictionary<string, CheckResult>();

                    #region CheckResult
                    foreach (var jobResult in jobResultList)
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("SELECT * FROM CheckResult WHERE JobUniqueID = '{0}'", jobResult.JobUniqueID); 

                            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                            {
                                using (DataTable dt = new DataTable())
                                {
                                    adapter.Fill(dt);

                                    if (dt != null && dt.Rows.Count > 0)
                                    {
                                        using (EDbEntities db = new EDbEntities())
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var uniqueID = dt.Rows[i]["UniqueID"].ToString();

                                                if (!db.CheckResult.Any(x => x.UniqueID == uniqueID))
                                                {
                                                    var jobUniqueID = dt.Rows[i]["JobUniqueID"].ToString();
                                                    var controlPointUniqueID = dt.Rows[i]["ControlPointUniqueID"].ToString();
                                                    var equipmentUniqueID = dt.Rows[i]["EquipmentUniqueID"].ToString();
                                                    var partUniqueID = dt.Rows[i]["PartUniqueID"].ToString();
                                                    var checkItemUniqueID = dt.Rows[i]["CheckItemUniqueID"].ToString();

                                                    if (string.IsNullOrEmpty(equipmentUniqueID))
                                                    {
                                                        var query = (from x in db.JobControlPointCheckItem
                                                                     join checkItem in db.View_ControlPointCheckItem
                                                                     on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { checkItem.ControlPointUniqueID, checkItem.CheckItemUniqueID }
                                                                     join item in db.CheckItem
                                                                     on x.CheckItemUniqueID equals item.UniqueID
                                                                     join c in db.ControlPoint
                                                                     on x.ControlPointUniqueID equals c.UniqueID
                                                                     join j in db.Job
                                                                     on x.JobUniqueID equals j.UniqueID
                                                                     join r in db.Route
                                                                     on j.RouteUniqueID equals r.UniqueID
                                                                     where x.JobUniqueID == jobUniqueID && x.ControlPointUniqueID == controlPointUniqueID && x.CheckItemUniqueID == checkItemUniqueID
                                                                     select new
                                                                     {
                                                                         r.OrganizationUniqueID,
                                                                         RouteUniqueID = r.UniqueID,
                                                                         RouteID = r.ID,
                                                                         RouteName = r.Name,
                                                                         JobUniqueID = j.UniqueID,
                                                                         JobDescription = j.Description,
                                                                         ControlPointUniqueID = c.UniqueID,
                                                                         ControlPointID = c.ID,
                                                                         ControlPointDescription = c.Description,
                                                                         CheckItemUniqueID = checkItem.CheckItemUniqueID,
                                                                         CheckItemID = checkItem.ID,
                                                                         IsFeelItem = checkItem.IsFeelItem,
                                                                         TextValueType = item.TextValueType,
                                                                         CheckItemDescription = checkItem.Description,
                                                                         checkItem.LowerLimit,
                                                                         checkItem.LowerAlertLimit,
                                                                         checkItem.UpperAlertLimit,
                                                                         checkItem.UpperLimit,
                                                                         checkItem.AccumulationBase,
                                                                         checkItem.Unit
                                                                     }).FirstOrDefault();

                                                        if (query != null)
                                                        {
                                                            var checkResult = new CheckResult()
                                                            {
                                                                UniqueID = uniqueID,
                                                                ArriveRecordUniqueID = dt.Rows[i]["ArriveRecordUniqueID"].ToString(),
                                                                OrganizationUniqueID = query.OrganizationUniqueID,
                                                                JobUniqueID = query.JobUniqueID,
                                                                JobDescription = query.JobDescription,
                                                                RouteUniqueID = query.RouteUniqueID,
                                                                RouteID = query.RouteID,
                                                                RouteName = query.RouteName,
                                                                ControlPointUniqueID = query.ControlPointUniqueID,
                                                                ControlPointID = query.ControlPointID,
                                                                ControlPointDescription = query.ControlPointDescription,
                                                                EquipmentUniqueID = "",
                                                                EquipmentID = "",
                                                                EquipmentName = "",
                                                                PartUniqueID = "",
                                                                PartDescription = "",
                                                                CheckItemUniqueID = query.CheckItemUniqueID,
                                                                CheckItemID = query.CheckItemID,
                                                                CheckItemDescription = query.CheckItemDescription,
                                                                LowerLimit = query.LowerLimit,
                                                                LowerAlertLimit = query.LowerAlertLimit,
                                                                UpperAlertLimit = query.UpperAlertLimit,
                                                                UpperLimit = query.UpperLimit,
                                                                Unit = query.Unit,
                                                                CheckDate = dt.Rows[i]["CheckDate"].ToString(),
                                                                CheckTime = dt.Rows[i]["CheckTime"].ToString(),
                                                                FeelOptionUniqueID = dt.Rows[i]["FeelOptionUniqueID"].ToString(),
                                                                IsAbnormal = false,
                                                                IsAlert = false
                                                            };

                                                            try
                                                            {
                                                                var remark = dt.Rows[i]["Remark"].ToString();

                                                                checkResult.Remark = remark;
                                                            }
                                                            catch
                                                            { }

                                                            var otherMk = dt.Rows[i]["OtherMk"].ToString();

                                                            if (!string.IsNullOrEmpty(otherMk))
                                                            {
                                                                checkResult.OtherMk = otherMk;

                                                                switch (otherMk)
                                                                {
                                                                    case "1":
                                                                        checkResult.OtherMkDescription = Resources.Resource.OtherMk1;
                                                                        break;
                                                                    case "2":
                                                                        checkResult.OtherMkDescription = Resources.Resource.OtherMk2;
                                                                        break;
                                                                    case "3":
                                                                        checkResult.OtherMkDescription = Resources.Resource.OtherMk3;
                                                                        break;
                                                                }

                                                                checkResult.Result = checkResult.OtherMkDescription;
                                                            }



                                                            if (!query.IsFeelItem)
                                                            {
                                                                if (query.TextValueType == 1)
                                                                {
                                                                    var value = dt.Rows[i]["Value"].ToString();

                                                                    if (!string.IsNullOrEmpty(value))
                                                                    {
                                                                        double val = double.Parse(value);
                                                                        double netVal = val;

                                                                        checkResult.Value = val;

                                                                        if (query.AccumulationBase.HasValue)
                                                                        {
                                                                            var prevCheckResult = db.CheckResult.Where(x => x.ControlPointUniqueID == query.ControlPointUniqueID && x.CheckItemUniqueID == query.CheckItemUniqueID && x.Value.HasValue).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                                                                            if (prevCheckResult != null)
                                                                            {
                                                                                netVal = val - prevCheckResult.Value.Value;
                                                                                checkResult.NetValue = netVal;
                                                                            }
                                                                            else
                                                                            {
                                                                                netVal = val - query.AccumulationBase.Value;
                                                                                checkResult.NetValue = netVal;
                                                                            }
                                                                        }

                                                                        if (!string.IsNullOrEmpty(checkResult.OtherMk))
                                                                        {
                                                                            checkResult.Result = string.Format("{0}({1})", checkResult.OtherMkDescription, netVal.ToString("F2"));
                                                                        }
                                                                        else
                                                                        {
                                                                            checkResult.Result = netVal.ToString("F2");
                                                                        }

                                                                        if (query.UpperLimit.HasValue && netVal > query.UpperLimit.Value)
                                                                        {
#if ORACLE
                                                                checkResult.IsAbnormal = 1;
#else
                                                                            checkResult.IsAbnormal = true;
#endif
                                                                        }

                                                                        if (query.UpperAlertLimit.HasValue && netVal > query.UpperAlertLimit.Value)
                                                                        {
#if ORACLE
                                                                checkResult.IsAlert = 1;
#else
                                                                            checkResult.IsAlert = true;
#endif
                                                                        }

                                                                        if (query.LowerLimit.HasValue && netVal < query.LowerLimit.Value)
                                                                        {
#if ORACLE
                                                                checkResult.IsAbnormal = 1;
#else
                                                                            checkResult.IsAbnormal = true;
#endif
                                                                        }

                                                                        if (query.LowerAlertLimit.HasValue && netVal < query.LowerAlertLimit.Value)
                                                                        {
#if ORACLE
                                                                checkResult.IsAlert = 1;
#else
                                                                            checkResult.IsAlert = true;
#endif
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    var textValue = dt.Rows[i]["TextValue"].ToString();

                                                                    if (!string.IsNullOrEmpty(checkResult.OtherMk))
                                                                    {
                                                                        checkResult.Result = string.Format("{0}({1})", checkResult.OtherMkDescription, textValue);
                                                                    }
                                                                    else
                                                                    {
                                                                        checkResult.Result = textValue;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                var feelOption = db.CheckItemFeelOption.FirstOrDefault(x => x.UniqueID == checkResult.FeelOptionUniqueID);

                                                                if (feelOption != null)
                                                                {
                                                                    checkResult.FeelOptionDescription = feelOption.Description;
                                                                    checkResult.IsAbnormal = feelOption.IsAbnormal;

                                                                    if (!string.IsNullOrEmpty(checkResult.OtherMk))
                                                                    {
                                                                        checkResult.Result = string.Format("{0}({1})", checkResult.OtherMkDescription, feelOption.Description);
                                                                    }
                                                                    else
                                                                    {
                                                                        checkResult.Result = feelOption.Description;
                                                                    }
                                                                }
                                                            }

                                                            if (checkResult.IsAbnormal)
                                                            {
                                                                var abnormalUniqueID = Guid.NewGuid().ToString();

                                                                db.Abnormal.Add(new Abnormal()
                                                                {
                                                                    UniqueID = abnormalUniqueID
                                                                });

                                                                db.AbnormalCheckResult.Add(new AbnormalCheckResult()
                                                                {
                                                                    AbnormalUniqueID = abnormalUniqueID,
                                                                    CheckResultUniqueID = checkResult.UniqueID
                                                                });

                                                                abnormalList.Add(abnormalUniqueID, checkResult);
                                                            }

                                                            db.CheckResult.Add(checkResult);

                                                            TransCheckResultPhoto(db, conn, uniqueID, extractPath);

                                                            TransCheckResultAbnormalReason(db, conn, uniqueID);

                                                            TransCheckResultHandlingMethod(db, conn, uniqueID);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var query = (from x in db.JobEquipmentCheckItem
                                                                     join checkItem in db.View_EquipmentCheckItem
                                                                     on new { x.EquipmentUniqueID, x.PartUniqueID, x.CheckItemUniqueID } equals new { checkItem.EquipmentUniqueID, checkItem.PartUniqueID, checkItem.CheckItemUniqueID }
                                                                     join item in db.CheckItem
                                                                     on x.CheckItemUniqueID equals item.UniqueID
                                                                     join e in db.Equipment
                                                                     on x.EquipmentUniqueID equals e.UniqueID
                                                                     join p in db.EquipmentPart
                                                                     on new { x.EquipmentUniqueID, x.PartUniqueID } equals new { p.EquipmentUniqueID, PartUniqueID = p.UniqueID } into tmpPart
                                                                     from p in tmpPart.DefaultIfEmpty()
                                                                     join controlPoint in db.ControlPoint
                                                                     on x.ControlPointUniqueID equals controlPoint.UniqueID
                                                                     join job in db.Job
                                                                    on x.JobUniqueID equals job.UniqueID
                                                                     join route in db.Route
                                                                      on job.RouteUniqueID equals route.UniqueID
                                                                     where x.JobUniqueID == jobUniqueID && x.ControlPointUniqueID == controlPointUniqueID && x.EquipmentUniqueID == equipmentUniqueID && x.PartUniqueID == partUniqueID && x.CheckItemUniqueID == checkItemUniqueID && uploadList.Contains(job.UniqueID)//UploadDefine
                                                                     select new
                                                                     {
                                                                         route.OrganizationUniqueID,
                                                                         JobUniqueID = job.UniqueID,
                                                                         JobDescription = job.Description,
                                                                         RouteUniqueID = route.UniqueID,
                                                                         RouteID = route.ID,
                                                                         RouteName = route.Name,
                                                                         ControlPointUniqueID = controlPoint.UniqueID,
                                                                         ControlPointID = controlPoint.ID,
                                                                         ControlPointDescription = controlPoint.Description,
                                                                         EquipmentUniqueID = e.UniqueID,
                                                                         EquipmentID = e.ID,
                                                                         EquipmentName = e.Name,
                                                                         PartUniqueID = x.PartUniqueID,
                                                                         PartDescription = p != null ? p.Description : "",
                                                                         CheckItemUniqueID = checkItem.CheckItemUniqueID,
                                                                         CheckItemID = checkItem.ID,
                                                                         CheckItemDescription = checkItem.Description,
                                                                         IsFeelItem = item.IsFeelItem,
                                                                         TextValueType = item.TextValueType,
                                                                         checkItem.LowerLimit,
                                                                         checkItem.LowerAlertLimit,
                                                                         checkItem.UpperAlertLimit,
                                                                         checkItem.UpperLimit,
                                                                         checkItem.AccumulationBase,
                                                                         checkItem.Unit
                                                                     }).FirstOrDefault();

                                                        if (query != null)
                                                        {
                                                            var checkResult = new CheckResult()
                                                            {
                                                                UniqueID = uniqueID,
                                                                ArriveRecordUniqueID = dt.Rows[i]["ArriveRecordUniqueID"].ToString(),
                                                                OrganizationUniqueID = query.OrganizationUniqueID,
                                                                JobUniqueID = query.JobUniqueID,
                                                                JobDescription = query.JobDescription,
                                                                RouteUniqueID = query.RouteUniqueID,
                                                                RouteID = query.RouteID,
                                                                RouteName = query.RouteName,
                                                                ControlPointUniqueID = query.ControlPointUniqueID,
                                                                ControlPointID = query.ControlPointID,
                                                                ControlPointDescription = query.ControlPointDescription,
                                                                EquipmentUniqueID = query.EquipmentUniqueID,
                                                                EquipmentID = query.EquipmentID,
                                                                EquipmentName = query.EquipmentName,
                                                                PartUniqueID = query.PartUniqueID,
                                                                PartDescription = query.PartDescription,
                                                                CheckItemUniqueID = query.CheckItemUniqueID,
                                                                CheckItemID = query.CheckItemID,
                                                                CheckItemDescription = query.CheckItemDescription,
                                                                LowerLimit = query.LowerLimit,
                                                                LowerAlertLimit = query.LowerAlertLimit,
                                                                UpperAlertLimit = query.UpperAlertLimit,
                                                                UpperLimit = query.UpperLimit,
                                                                Unit = query.Unit,
                                                                CheckDate = dt.Rows[i]["CheckDate"].ToString(),
                                                                CheckTime = dt.Rows[i]["CheckTime"].ToString(),
                                                                FeelOptionUniqueID = dt.Rows[i]["FeelOptionUniqueID"].ToString(),
#if ORACLE
                                                            IsAbnormal = 0
#else
                                                                IsAbnormal = false
#endif
                                                            };



                                                            var otherMk = dt.Rows[i]["OtherMk"].ToString();

                                                            if (!string.IsNullOrEmpty(otherMk))
                                                            {
                                                                checkResult.OtherMk = otherMk;

                                                                switch (otherMk)
                                                                {
                                                                    case "1":
                                                                        checkResult.OtherMkDescription = Resources.Resource.OtherMk1;
                                                                        break;
                                                                    case "2":
                                                                        checkResult.OtherMkDescription = Resources.Resource.OtherMk2;
                                                                        break;
                                                                    case "3":
                                                                        checkResult.OtherMkDescription = Resources.Resource.OtherMk3;
                                                                        break;
                                                                }

                                                                checkResult.Result = checkResult.OtherMkDescription;
                                                            }

                                                            if (!query.IsFeelItem)
                                                            {
                                                                if (query.TextValueType == 1)
                                                                {
                                                                    var value = dt.Rows[i]["Value"].ToString();

                                                                    if (!string.IsNullOrEmpty(value))
                                                                    {
                                                                        double val = double.Parse(value);
                                                                        double netVal = val;

                                                                        checkResult.Value = val;

                                                                        if (query.AccumulationBase.HasValue)
                                                                        {
                                                                            var prevCheckResult = db.CheckResult.Where(x => x.EquipmentUniqueID == query.EquipmentUniqueID && query.PartUniqueID == query.PartUniqueID && x.CheckItemUniqueID == query.CheckItemUniqueID && x.Value.HasValue).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                                                                            if (prevCheckResult != null)
                                                                            {
                                                                                netVal = val - prevCheckResult.Value.Value;
                                                                                checkResult.NetValue = netVal;
                                                                            }
                                                                            else
                                                                            {
                                                                                netVal = val - query.AccumulationBase.Value;
                                                                                checkResult.NetValue = netVal;
                                                                            }
                                                                        }

                                                                        if (!string.IsNullOrEmpty(checkResult.OtherMk))
                                                                        {
                                                                            checkResult.Result = string.Format("{0}({1})", checkResult.OtherMkDescription, netVal.ToString("F2"));
                                                                        }
                                                                        else
                                                                        {
                                                                            checkResult.Result = netVal.ToString("F2");
                                                                        }

                                                                        if (query.UpperLimit.HasValue && netVal > query.UpperLimit.Value)
                                                                        {
#if ORACLE
                                                                checkResult.IsAbnormal = 1;
#else
                                                                            checkResult.IsAbnormal = true;
#endif
                                                                        }

                                                                        if (query.UpperAlertLimit.HasValue && netVal > query.UpperAlertLimit.Value)
                                                                        {
#if ORACLE
                                                                checkResult.IsAlert = 1;
#else
                                                                            checkResult.IsAlert = true;
#endif
                                                                        }

                                                                        if (query.LowerLimit.HasValue && netVal < query.LowerLimit.Value)
                                                                        {
#if ORACLE
                                                                checkResult.IsAbnormal = 1;
#else
                                                                            checkResult.IsAbnormal = true;
#endif
                                                                        }

                                                                        if (query.LowerAlertLimit.HasValue && netVal < query.LowerAlertLimit.Value)
                                                                        {
#if ORACLE
                                                                checkResult.IsAlert = 1;
#else
                                                                            checkResult.IsAlert = true;
#endif
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    var textValue = dt.Rows[i]["TextValue"].ToString();

                                                                    checkResult.Result = textValue;

                                                                    if (!string.IsNullOrEmpty(checkResult.OtherMk))
                                                                    {
                                                                        checkResult.Result = string.Format("{0}({1})", checkResult.OtherMkDescription, textValue);
                                                                    }
                                                                    else
                                                                    {
                                                                        checkResult.Result = textValue;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                var feelOption = db.CheckItemFeelOption.FirstOrDefault(x => x.UniqueID == checkResult.FeelOptionUniqueID);

                                                                if (feelOption != null)
                                                                {
                                                                    checkResult.FeelOptionDescription = feelOption.Description;
                                                                    checkResult.IsAbnormal = feelOption.IsAbnormal;

                                                                    if (!string.IsNullOrEmpty(checkResult.OtherMk))
                                                                    {
                                                                        checkResult.Result = string.Format("{0}({1})", checkResult.OtherMkDescription, feelOption.Description);
                                                                    }
                                                                    else
                                                                    {
                                                                        checkResult.Result = feelOption.Description;
                                                                    }
                                                                }
                                                            }

                                                            if (checkResult.IsAbnormal)
                                                            {
                                                                var abnormalUniqueID = Guid.NewGuid().ToString();

                                                                db.Abnormal.Add(new Abnormal()
                                                                {
                                                                    UniqueID = abnormalUniqueID
                                                                });

                                                                db.AbnormalCheckResult.Add(new AbnormalCheckResult()
                                                                {
                                                                    AbnormalUniqueID = abnormalUniqueID,
                                                                    CheckResultUniqueID = checkResult.UniqueID
                                                                });

                                                                abnormalList.Add(abnormalUniqueID, checkResult);
                                                            }

                                                            db.CheckResult.Add(checkResult);

                                                            TransCheckResultPhoto(db, conn, uniqueID, extractPath);

                                                            TransCheckResultAbnormalReason(db, conn, uniqueID);

                                                            TransCheckResultHandlingMethod(db, conn, uniqueID);
                                                        }
                                                    }
                                                }
                                            }

                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }                    
                    }
                    #endregion

                    if (Config.HaveMailSetting && abnormalList.Count > 0)
                    {
                        SendAbnormalMail(abnormalList);
                    }

                    #region OverTimeRecord
                    foreach (var jobResult in jobResultList)
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("SELECT * FROM OverTimeRecord WHERE JobUniqueID = '{0}'", jobResult.JobUniqueID);

                            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                            {
                                using (DataTable dt = new DataTable())
                                {
                                    adapter.Fill(dt);

                                    if (dt != null && dt.Rows.Count > 0)
                                    {
                                        using (EDbEntities db = new EDbEntities())
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var overTimeReasonUniqueID = dt.Rows[i]["OverTimeReasonUniqueID"].ToString();
                                                var userID = dt.Rows[i]["UserID"].ToString();

                                                var abnormalReason = db.OverTimeReason.FirstOrDefault(x => x.UniqueID == overTimeReasonUniqueID);

                                                jobResult.NewOverTimeReasonDescription = abnormalReason != null ? abnormalReason.Description : (overTimeReasonUniqueID == Define.OTHER ? Resources.Resource.Other : string.Empty);
                                                jobResult.NewOverTimeReasonRemark = dt.Rows[i]["OverTimeReasonRemark"].ToString();
                                                jobResult.NewOverTimeUser = UserDataAccessor.GetUser(userID);
                                            }

                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region UnPatrolRecord
                    foreach (var jobResult in jobResultList)
                    {
                        try
                        {
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = string.Format("SELECT * FROM UnPatrolRecord WHERE JobUniqueID = '{0}'", jobResult.JobUniqueID);

                                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                                {
                                    using (DataTable dt = new DataTable())
                                    {
                                        adapter.Fill(dt);

                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                            using (EDbEntities db = new EDbEntities())
                                            {
                                                for (int i = 0; i < dt.Rows.Count; i++)
                                                {
                                                    var unPatrolReasonUniqueID = dt.Rows[i]["UnPatrolReasonUniqueID"].ToString();
                                                    var userID = dt.Rows[i]["UserID"].ToString();

                                                    var abnormalReason = db.UnPatrolReason.FirstOrDefault(x => x.UniqueID == unPatrolReasonUniqueID);

                                                    jobResult.NewUnPatrolReasonDescription = abnormalReason != null ? abnormalReason.Description : (unPatrolReasonUniqueID == Define.OTHER ? Resources.Resource.Other : string.Empty);
                                                    jobResult.NewUnPatrolReasonRemark = dt.Rows[i]["UnPatrolReasonRemark"].ToString();
                                                    jobResult.NewUnPatrolUser = UserDataAccessor.GetUser(userID);
                                                }

                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        { }
                    }
                    #endregion

                    RefreshJobResult(jobResultList);

                    if (Config.Modules.Contains(Define.EnumSystemModule.EquipmentMaintenance))
                    {
                        var mFormResultUser = new Dictionary<string, string>();

                        #region MFormResult
                        foreach (var mformUniqueID in uploadList)
                        {
                            var resultUniqueID = Guid.NewGuid().ToString();

                            #region MFormResult
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = string.Format("SELECT * FROM MFormResult WHERE MFormUniqueID = '{0}'", mformUniqueID);

                                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                                {
                                    using (DataTable dt = new DataTable())
                                    {
                                        adapter.Fill(dt);

                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                            using (DbEntities db = new DbEntities())
                                            {
                                                using (EDbEntities edb = new EDbEntities())
                                                {
                                                    for (int i = 0; i < dt.Rows.Count; i++)
                                                    {
                                                        var form = edb.MForm.FirstOrDefault(x => x.UniqueID == mformUniqueID);

                                                        if (form != null)
                                                        {
                                                            var userID = dt.Rows[i]["USERID"].ToString();
                                                            var date = dt.Rows[i]["MDATE"].ToString();
                                                            var time = dt.Rows[i]["MTIME"].ToString();
                                                            var remark = dt.Rows[i]["REMARK"].ToString();
                                                            //var isNeedVerify = dt.Rows[i]["IsNeedVerify"].ToString();

                                                            mFormResultUser.Add(mformUniqueID, userID);

                                                            if (form.Status == "0")
                                                            {
                                                                form.Status = "1";
                                                                form.TakeJobTime = DateTime.Now;
                                                                form.TakeJobUserID = userID;
                                                            }

                                                            var user = db.User.FirstOrDefault(x => x.ID == userID);

                                                            edb.MFormResult.Add(new MFormResult()
                                                            {
                                                                UniqueID = resultUniqueID,
                                                                PMDate = date,
                                                                MFormUniqueID = mformUniqueID,
                                                                PMTime = time,
                                                                UserID = userID,
                                                                UserName = user != null ? user.Name : "",
                                                                JobRemark = remark
                                                            });

                                                            edb.SaveChanges();

                                                            //if (isNeedVerify == "Y")
                                                            //{
                                                            //    MaintenanceFormDataAccessor.Submit(mformUniqueID);
                                                            //}
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region MFormWorkingHour
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = string.Format("SELECT * FROM MFormWorkingHour WHERE MFormUniqueID = '{0}'", mformUniqueID);

                                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                                {
                                    using (DataTable dt = new DataTable())
                                    {
                                        adapter.Fill(dt);

                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                            using (EDbEntities db = new EDbEntities())
                                            {
                                                for (int i = 0; i < dt.Rows.Count; i++)
                                                {
                                                    if (mFormResultUser.Any(x => x.Key == mformUniqueID) && dt.Rows[i]["WorkingHour"] != null && !string.IsNullOrEmpty(dt.Rows[i]["WorkingHour"].ToString()))
                                                    {
                                                        var seq = 1;

                                                        var workingHours = db.MFormWorkingHour.Where(x => x.MFormUniqueID == mformUniqueID).ToList();

                                                        if (workingHours.Count > 0)
                                                        {
                                                            seq = workingHours.Max(x => x.Seq) + 1;
                                                        }

                                                        db.MFormWorkingHour.Add(new MFormWorkingHour()
                                                        {
                                                            MFormUniqueID = mformUniqueID,
                                                            Seq = seq,
                                                            UserID = mFormResultUser[mformUniqueID],
                                                            BeginDate = dt.Rows[i]["BeginDate"].ToString(),
                                                            EndDate = dt.Rows[i]["EndDate"].ToString(),
                                                            WorkingHour = double.Parse(dt.Rows[i]["WorkingHour"].ToString())
                                                        });
                                                    }
                                                }

                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region MFormMaterialResult
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = string.Format("SELECT * FROM MFormMaterialResult WHERE MFormUniqueID = '{0}'", mformUniqueID);

                                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                                {
                                    using (DataTable dt = new DataTable())
                                    {
                                        adapter.Fill(dt);

                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                            using (EDbEntities db = new EDbEntities())
                                            {
                                                for (int i = 0; i < dt.Rows.Count; i++)
                                                {
                                                    var mFormUniqueID = dt.Rows[i]["MFormUniqueID"].ToString();
                                                    var materialUniqueID = dt.Rows[i]["MaterialUniqueID"].ToString();
                                                    var qty = int.Parse(dt.Rows[i]["Quantity"].ToString());

                                                    var materialResult = db.MFormMaterialResult.FirstOrDefault(x => x.MFormUniqueID == mFormUniqueID && x.MaterialUniqueID == materialUniqueID);

                                                    if (materialResult == null)
                                                    {
                                                        var query = (from f in db.MForm
                                                                     join e in db.Equipment
                                                                     on f.EquipmentUniqueID equals e.UniqueID
                                                                     join p in db.EquipmentPart
                                                                     on f.PartUniqueID equals p.UniqueID into tmpPart
                                                                     from p in tmpPart.DefaultIfEmpty()
                                                                     join x in db.MJobEquipmentMaterial
                                                                     on new { f.MJobUniqueID, f.EquipmentUniqueID, f.PartUniqueID } equals new { x.MJobUniqueID, x.EquipmentUniqueID, x.PartUniqueID }
                                                                     join m in db.Material
                                                                     on x.MaterialUniqueID equals m.UniqueID
                                                                     where f.UniqueID == mformUniqueID && x.MaterialUniqueID == materialUniqueID
                                                                     select new
                                                                     {
                                                                         EquipmentUniqueID = f.EquipmentUniqueID,
                                                                         EquipmentID = e.ID,
                                                                         EquipmentName = e.Name,
                                                                         PartUniqueID = f.PartUniqueID,
                                                                         PartDescription = p != null ? p.Description : "",
                                                                         MaterialUniqueID = x.MaterialUniqueID,
                                                                         MaterialID = m.ID,
                                                                         MaterialName = m.Name,
                                                                         Quantity = x.Quantity
                                                                     }).First();

                                                        db.MFormMaterialResult.Add(new MFormMaterialResult()
                                                        {
                                                            MFormUniqueID = mFormUniqueID,
                                                            EquipmentUniqueID = query.EquipmentUniqueID,
                                                            EquipmentID = query.EquipmentID,
                                                            EquipmentName = query.EquipmentName,
                                                            MaterialUniqueID = materialUniqueID,
                                                            MaterialID = query.MaterialID,
                                                            MaterialName = query.MaterialName,
                                                            PartUniqueID = query.PartUniqueID,
                                                            PartDescription = query.PartDescription,
                                                            Quantity = query.Quantity.Value,
                                                            ChangeQuantity = qty
                                                        });
                                                    }
                                                    else
                                                    {
                                                        materialResult.ChangeQuantity = qty;
                                                    }
                                                }

                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region MFormStandardResult
                            using (SQLiteCommand cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = string.Format("SELECT * FROM MFormStandardResult WHERE MFormUniqueID = '{0}'", mformUniqueID);

                                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                                {
                                    using (DataTable dt = new DataTable())
                                    {
                                        adapter.Fill(dt);

                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                            using (EDbEntities db = new EDbEntities())
                                            {
                                                for (int i = 0; i < dt.Rows.Count; i++)
                                                {
                                                    var standardUniqueID = dt.Rows[i]["StandardUniqueID"].ToString();

                                                    var query = (from x in db.MForm
                                                                 join j in db.MJob
                                                                 on x.MJobUniqueID equals j.UniqueID
                                                                 join e in db.Equipment
                                                                 on x.EquipmentUniqueID equals e.UniqueID
                                                                 join p in db.EquipmentPart
                                                                 on x.PartUniqueID equals p.UniqueID into tmpPart
                                                                 from p in tmpPart.DefaultIfEmpty()
                                                                 join y in db.MJobEquipmentStandard
                                                                 on new { x.MJobUniqueID, x.EquipmentUniqueID, x.PartUniqueID } equals new { y.MJobUniqueID, y.EquipmentUniqueID, y.PartUniqueID }
                                                                 join z in db.EquipmentStandard
                                                                 on new { y.EquipmentUniqueID, y.PartUniqueID } equals new { z.EquipmentUniqueID, z.PartUniqueID }
                                                                 join s in db.Standard
                                                                 on y.StandardUniqueID equals s.UniqueID
                                                                 where x.UniqueID == mformUniqueID && s.UniqueID == standardUniqueID
                                                                 select new
                                                                 {
                                                                     MJobUniqueID = j.UniqueID,
                                                                     MJobDescription = j.Description,
                                                                     OrganizationUniqueID = j.OrganizationUniqueID,
                                                                     EquipmentUniqueID = x.EquipmentUniqueID,
                                                                     EquipmentID = e.ID,
                                                                     EquipmentName = e.Name,
                                                                     PartUniqueID = x.PartUniqueID,
                                                                     PartDescription = p != null ? p.Description : "",
                                                                     StandardUniqueID = s.UniqueID,
                                                                     StandardID = s.ID,
                                                                     StandardDescription = s.Description,
                                                                     LowerLimit = z.IsInherit ? s.LowerLimit : z.LowerLimit,
                                                                     LowerAlertLimit = z.IsInherit ? s.LowerAlertLimit : z.LowerAlertLimit,
                                                                     UpperAlertLimit = z.IsInherit ? s.UpperAlertLimit : z.UpperAlertLimit,
                                                                     UpperLimit = z.IsInherit ? s.UpperLimit : z.UpperLimit,
                                                                     Unit = z.IsInherit ? s.Unit : z.Unit,
                                                                     AccumulationBase = z.IsInherit ? s.AccumulationBase : z.AccumulationBase,
                                                                     IsAccumulation = s.IsAccumulation,
                                                                 }).FirstOrDefault();

                                                    if (query != null)
                                                    {
                                                        var standardResult = new MFormStandardResult()
                                                        {
                                                            UniqueID = Guid.NewGuid().ToString(),
                                                            ResultUniqueID = resultUniqueID,
                                                            MFormUniqueID = mformUniqueID,
                                                            OrganizationUniqueID = query.OrganizationUniqueID,
                                                            MJobUniqueID = query.MJobUniqueID,
                                                            MJobDescription = query.MJobDescription,
                                                            EquipmentUniqueID = query.EquipmentUniqueID,
                                                            EquipmentID = query.EquipmentID,
                                                            EquipmentName = query.EquipmentName,
                                                            PartUniqueID = query.PartUniqueID,
                                                            PartDescription = query.PartDescription,
                                                            StandardUniqueID = query.StandardUniqueID,
                                                            StandardID = query.StandardID,
                                                            StandardDescription = query.StandardDescription,
                                                            FeelOptionUniqueID = dt.Rows[i]["FeelOptionUniqueID"].ToString(),
                                                            IsAbnormal = false,
                                                            IsAlert = false,
                                                            LowerAlertLimit = query.LowerAlertLimit,
                                                            LowerLimit = query.LowerLimit,
                                                            Unit = query.Unit,
                                                            UpperAlertLimit = query.UpperAlertLimit,
                                                            UpperLimit = query.UpperLimit
                                                        };

                                                        var value = dt.Rows[i]["Value"].ToString();

                                                        if (!string.IsNullOrEmpty(value))
                                                        {
                                                            decimal val = decimal.Round(decimal.Parse(value), 5);
                                                            decimal netVal = val;

                                                            standardResult.Value = Convert.ToDouble(val);

                                                            if (query.IsAccumulation)
                                                            {
                                                                var prevStandardResult = (from x in db.MFormStandardResult
                                                                                          join r in db.MFormResult
                                                                                          on x.ResultUniqueID equals r.UniqueID
                                                                                          where x.EquipmentUniqueID == query.EquipmentUniqueID && x.PartUniqueID == query.PartUniqueID && x.StandardUniqueID == standardUniqueID && x.Value.HasValue
                                                                                          select new { r.PMDate, r.PMTime, x.Value }).OrderByDescending(x => x.PMDate).ThenByDescending(x => x.PMTime).FirstOrDefault();

                                                                if (prevStandardResult != null)
                                                                {
                                                                    netVal = decimal.Round(val - Convert.ToDecimal(prevStandardResult.Value.Value), 5);
                                                                    standardResult.NetValue = Convert.ToDouble(netVal);
                                                                }
                                                                else
                                                                {
                                                                    netVal = decimal.Round(val - Convert.ToDecimal(query.AccumulationBase.Value), 5);
                                                                    standardResult.NetValue = Convert.ToDouble(netVal);
                                                                }
                                                            }

                                                            standardResult.Result = netVal.ToString("F2");

                                                            if (query.UpperLimit.HasValue && netVal > Convert.ToDecimal(query.UpperLimit.Value))
                                                            {
                                                                standardResult.IsAbnormal = true;
                                                            }

                                                            if (query.UpperAlertLimit.HasValue && netVal > Convert.ToDecimal(query.UpperAlertLimit.Value))
                                                            {
                                                                standardResult.IsAlert = true;
                                                            }

                                                            if (query.LowerLimit.HasValue && netVal < Convert.ToDecimal(query.LowerLimit.Value))
                                                            {
                                                                standardResult.IsAbnormal = true;
                                                            }

                                                            if (query.LowerAlertLimit.HasValue && netVal < Convert.ToDecimal(query.LowerAlertLimit.Value))
                                                            {
                                                                standardResult.IsAlert = true;
                                                            }
                                                        }

                                                        if (!string.IsNullOrEmpty(standardResult.FeelOptionUniqueID))
                                                        {
                                                            var feelOption = db.StandardFeelOption.FirstOrDefault(x => x.UniqueID == standardResult.FeelOptionUniqueID);

                                                            if (feelOption != null)
                                                            {
                                                                standardResult.IsAbnormal = feelOption.IsAbnormal;

                                                                standardResult.Result = feelOption.Description;
                                                            }
                                                        }

                                                        db.MFormStandardResult.Add(standardResult);

                                                        //TransStandardResultPhoto(db, conn, mFormUniqueID, standardUniqueID, extractPath);

                                                        //TransStandardResultAbnormalReason(db, conn, mFormUniqueID, standardUniqueID);

                                                        //TransStandardResultHandlingMethod(db, conn, mFormUniqueID, standardUniqueID);
                                                    }
                                                }

                                                db.SaveChanges();
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                    
                    conn.Close();
                }

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return result;
        }

        private void TransArriveRecordPhoto(EDbEntities DB, SQLiteConnection Conn, string ArriveRecordUniqueID, string ExtractPath)
        {
            using (SQLiteCommand cmd = Conn.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM ArriveRecordPhoto WHERE ArriveRecordUniqueID = '{0}'", ArriveRecordUniqueID);

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                {
                    using (DataTable dt = new DataTable())
                    {
                        adapter.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                var photo = Path.Combine(ExtractPath, dt.Rows[i]["FileName"].ToString());

                                if (System.IO.File.Exists(photo))
                                {
                                    var extension = new FileInfo(photo).Extension.Substring(1);

                                    System.IO.File.Copy(photo, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, ArriveRecordUniqueID + "_" + (i + 1).ToString() + "." + extension), true);

                                    DB.ArriveRecordPhoto.Add(new ArriveRecordPhoto()
                                    {
                                        ArriveRecordUniqueID = ArriveRecordUniqueID,
                                        Seq = i + 1,
                                        Extension = extension
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TransCheckResultPhoto(EDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID, string ExtractPath)
        {
            using (SQLiteCommand cmd = Conn.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM CheckResultPhoto WHERE CheckResultUniqueID = '{0}'", CheckResultUniqueID);

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                {
                    using (DataTable dt = new DataTable())
                    {
                        adapter.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            int photoIndex = 2;

                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                var fileName = dt.Rows[j]["FileName"].ToString();

                                var photo = Path.Combine(ExtractPath, fileName);

                                if (System.IO.File.Exists(photo))
                                {
                                    var extension = new FileInfo(photo).Extension.Substring(1);

                                    if (fileName.StartsWith("FLIROne_"))
                                    {
                                        if (fileName.StartsWith("FLIROne_Org"))
                                        {
                                            System.IO.File.Copy(photo, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, CheckResultUniqueID + "_" + "1" + "." + extension), true);

                                            DB.CheckResultPhoto.Add(new CheckResultPhoto()
                                            {
                                                CheckResultUniqueID = CheckResultUniqueID,
                                                Seq = 1,
                                                Extension = extension
                                            });
                                        }
                                        else
                                        {
                                            System.IO.File.Copy(photo, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, CheckResultUniqueID + "_" + "0" + "." + extension), true);

                                            DB.CheckResultPhoto.Add(new CheckResultPhoto()
                                            {
                                                CheckResultUniqueID = CheckResultUniqueID,
                                                Seq = 0,
                                                Extension = extension
                                            });
                                        }
                                    }
                                    else
                                    {
                                        System.IO.File.Copy(photo, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, CheckResultUniqueID + "_" + photoIndex + "." + extension), true);

                                        DB.CheckResultPhoto.Add(new CheckResultPhoto()
                                        {
                                            CheckResultUniqueID = CheckResultUniqueID,
                                            Seq = photoIndex,
                                            Extension = extension
                                        });

                                        photoIndex++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TransCheckResultAbnormalReason(EDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID)
        {
            using (SQLiteCommand cmd = Conn.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM CheckResultAbnormalReason WHERE CheckResultUniqueID = '{0}'", CheckResultUniqueID);

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                {
                    using (DataTable dt = new DataTable())
                    {
                        adapter.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                var abnormalReasonUniqueID = dt.Rows[j]["AbnormalReasonUniqueID"].ToString();

                                var abnormalReason = DB.AbnormalReason.FirstOrDefault(x => x.UniqueID == abnormalReasonUniqueID);

                                DB.CheckResultAbnormalReason.Add(new CheckResultAbnormalReason()
                                {
                                    CheckResultUniqueID = CheckResultUniqueID,
                                    AbnormalReasonUniqueID = abnormalReasonUniqueID,
                                    AbnormalReasonID = abnormalReason != null ? abnormalReason.ID : (abnormalReasonUniqueID == "OTHER" ? "OTHER" : ""),
                                    AbnormalReasonDescription = abnormalReason != null ? abnormalReason.Description : (abnormalReasonUniqueID == "OTHER" ? Resources.Resource.Other : ""),
                                    AbnormalReasonRemark = dt.Rows[j]["AbnormalReasonRemark"].ToString()
                                });
                            }
                        }
                    }
                }
            }
        }

        private void TransCheckResultHandlingMethod(EDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID)
        {
            using (SQLiteCommand cmd = Conn.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM CheckResultHandlingMethod WHERE CheckResultUniqueID = '{0}'", CheckResultUniqueID);

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                {
                    using (DataTable handlingMethodDt = new DataTable())
                    {
                        adapter.Fill(handlingMethodDt);

                        if (handlingMethodDt != null && handlingMethodDt.Rows.Count > 0)
                        {
                            for (int j = 0; j < handlingMethodDt.Rows.Count; j++)
                            {
                                var handlingMethodUniqueID = handlingMethodDt.Rows[j]["HandlingMethodUniqueID"].ToString();

                                var handlingMethod = DB.HandlingMethod.FirstOrDefault(x => x.UniqueID == handlingMethodUniqueID);

                                DB.CheckResultHandlingMethod.Add(new CheckResultHandlingMethod()
                                {
                                    CheckResultUniqueID = CheckResultUniqueID,
                                    AbnormalReasonUniqueID = handlingMethodDt.Rows[j]["AbnormalReasonUniqueID"].ToString(),
                                    HandlingMethodUniqueID = handlingMethodUniqueID,
                                    HandlingMethodID = handlingMethod != null ? handlingMethod.ID : (handlingMethodUniqueID == "OTHER" ? "OTHER" : ""),
                                    HandlingMethodDescription = handlingMethod != null ? handlingMethod.Description : (handlingMethodUniqueID == "OTHER" ? Resources.Resource.Other : ""),
                                    HandlingMethodRemark = handlingMethodDt.Rows[j]["HandlingMethodRemark"].ToString()
                                });
                            }
                        }
                    }
                }
            }
        }

        private void SendAbnormalMail(Dictionary<string, CheckResult> AbnormalList)
        {
            try
            {
                var temp1 = AbnormalList.Values.Select(x => new { x.OrganizationUniqueID, x.RouteUniqueID, x.JobUniqueID }).Distinct().ToList();

                foreach (var t1 in temp1)
                {
                    try
                    {
                        var mailAddressList = new List<MailAddress>();

                        using (DbEntities db = new DbEntities())
                        {
                            using (EDbEntities edb = new EDbEntities())
                            {
                                var managerList = edb.RouteManager.Where(x => x.RouteUniqueID == t1.RouteUniqueID).Select(x => x.UserID).ToList();

                                var organization = db.Organization.FirstOrDefault(x => x.UniqueID == t1.OrganizationUniqueID);

                                if (organization != null)
                                {
                                    managerList.Add(organization.ManagerUserID);
                                }

                                foreach (var manager in managerList)
                                {
                                    var user = db.User.FirstOrDefault(x => x.ID == manager);

                                    if (user != null && !string.IsNullOrEmpty(user.Email))
                                    {
                                        mailAddressList.Add(new MailAddress(user.Email, user.Name));
                                    }
                                }


                                if (mailAddressList.Count > 0)
                                {
                                    var temp2 = AbnormalList.Values.Where(x => x.JobUniqueID == t1.JobUniqueID).Select(x => new
                                    {
                                        x.CheckDate,
                                        x.RouteID,
                                        x.RouteName,
                                        x.JobDescription
                                    }).Distinct().ToList();

                                    foreach (var t2 in temp2)
                                    {
                                        var route = string.Format("{0}/{1}-{2}", t2.RouteID, t2.RouteName, t2.JobDescription);

                                        var subject = string.Format("[{0}][{1}]{2}：{3}", Resources.Resource.CheckAbnormalNotify, t2.CheckDate, Resources.Resource.Route, route);

                                        var temp3 = AbnormalList.Values.Where(x => x.JobUniqueID == t1.JobUniqueID && x.CheckDate == t2.CheckDate).Select(x => new
                                        {
                                            x.UniqueID,
                                            x.ControlPointID,
                                            x.ControlPointDescription,
                                            x.EquipmentID,
                                            x.EquipmentName,
                                            x.PartDescription,
                                            x.CheckItemID,
                                            x.CheckItemDescription,
                                            x.Result,
                                            x.IsAbnormal,
                                            x.IsAlert,
                                            x.CheckDate,
                                            x.CheckTime,
                                            x.LowerLimit,
                                            x.LowerAlertLimit,
                                            x.UpperAlertLimit,
                                            x.UpperLimit,
                                            x.Unit
                                        }).ToList();

                                        var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                                        var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";

                                        var sb = new StringBuilder();

                                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                                        sb.Append("<tr>");

                                        sb.Append(string.Format(th, Resources.Resource.ControlPoint));
                                        sb.Append(string.Format(th, Resources.Resource.Equipment));
                                        sb.Append(string.Format(th, Resources.Resource.CheckItem));
                                        sb.Append(string.Format(th, Resources.Resource.CheckTime));
                                        sb.Append(string.Format(th, Resources.Resource.CheckResult));
                                        sb.Append(string.Format(th, Resources.Resource.Unit));
                                        sb.Append(string.Format(th, Resources.Resource.LowerLimit));
                                        sb.Append(string.Format(th, Resources.Resource.LowerAlertLimit));
                                        sb.Append(string.Format(th, Resources.Resource.UpperAlertLimit));
                                        sb.Append(string.Format(th, Resources.Resource.UpperLimit));
                                        sb.Append(string.Format(th, Resources.Resource.AbnormalReason + Resources.Resource.And + Resources.Resource.HandlingMethod));

                                        sb.Append("</tr>");

                                        foreach (var t3 in temp3)
                                        {
                                            var equipment = string.Empty;

                                            if (!string.IsNullOrEmpty(t3.EquipmentName))
                                            {
                                                if (!string.IsNullOrEmpty(t3.PartDescription))
                                                {
                                                    equipment = string.Format("{0}/{1}-{2}", t3.EquipmentID, t3.EquipmentName, t3.PartDescription);
                                                }
                                                else
                                                {
                                                    equipment = string.Format("{0}/{1}", t3.EquipmentID, t3.EquipmentName);
                                                }
                                            }
                                            else
                                            {
                                                equipment = t3.EquipmentID;
                                            }

                                            sb.Append("<tr>");

                                            sb.Append(string.Format(td, string.Format("{0}/{1}", t3.ControlPointID, t3.ControlPointDescription)));
                                            sb.Append(string.Format(td, equipment));
                                            sb.Append(string.Format(td, string.Format("{0}/{1}", t3.CheckItemID, t3.CheckItemDescription)));
                                            sb.Append(string.Format(td, DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(t3.CheckDate, t3.CheckTime))));
                                            //sb.Append(string.Format(td, DateTimeHelper.TimeString2TimeStringWithSeperator(t3.CheckTime)));
                                            sb.Append(string.Format(td, string.Format("{0}({1})", t3.Result, t3.IsAbnormal ? Resources.Resource.Abnormal : (t3.IsAlert ? Resources.Resource.Warning : string.Empty))));
                                            sb.Append(string.Format(td, t3.Unit));
                                            sb.Append(string.Format(td, t3.LowerLimit));
                                            sb.Append(string.Format(td, t3.LowerAlertLimit));
                                            sb.Append(string.Format(td, t3.UpperAlertLimit));
                                            sb.Append(string.Format(td, t3.UpperLimit));

                                            var abnormalReasonList = edb.CheckResultAbnormalReason.Where(a => a.CheckResultUniqueID == t3.UniqueID).OrderBy(a => a.AbnormalReasonID).Select(a => new AbnormalReasonModel
                                            {
                                                Description = a.AbnormalReasonDescription,
                                                Remark = a.AbnormalReasonRemark,
                                                HandlingMethodList = edb.CheckResultHandlingMethod.Where(h => h.CheckResultUniqueID == t3.UniqueID && h.AbnormalReasonUniqueID == a.AbnormalReasonUniqueID).OrderBy(h => h.HandlingMethodID).Select(h => new HandlingMethodModel
                                                {
                                                    Description = h.HandlingMethodDescription,
                                                    Remark = h.HandlingMethodRemark
                                                }).ToList()
                                            }).ToList();

                                            if (abnormalReasonList.Count > 0)
                                            {
                                                var t = new StringBuilder();

                                                foreach (var abnormalReason in abnormalReasonList)
                                                {
                                                    if (!string.IsNullOrEmpty(abnormalReason.HandlingMethods))
                                                    {
                                                        t.Append(string.Format("{0}({1})", abnormalReason.AbnormalReason, abnormalReason.HandlingMethods));
                                                    }
                                                    else
                                                    {
                                                        t.Append(abnormalReason.AbnormalReason);
                                                    }

                                                    t.Append("、");
                                                }

                                                t.Remove(t.Length - 1, 1);

                                                sb.Append(string.Format(td, t.ToString()));
                                            }
                                            else
                                            {
                                                sb.Append(string.Format(td, ""));
                                            }

                                            sb.Append("</tr>");
                                        }

                                        sb.Append("</table>");

                                        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(MethodBase.GetCurrentMethod(), string.Format("JobUniqueID:{0}", t1.JobUniqueID));
                        Logger.Log(MethodBase.GetCurrentMethod(), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void RefreshJobResult(List<JobResultModel> JobResultList)
        {
            try
            {
                foreach (var jobResult in JobResultList)
                {
                    JobResultDataAccessor.Refresh(jobResult.UniqueID, jobResult.JobUniqueID, jobResult.BeginDate, jobResult.EndDate, jobResult.OverTimeReason, jobResult.OverTimeUser, jobResult.UnPatrolReason, jobResult.UnPatrolUser);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        #region IDisposable

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }
            }

            _disposed = true;
        }

        ~TransHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
