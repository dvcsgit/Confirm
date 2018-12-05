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
using DataAccess;
using System.Net.Mail;
using System.Text;
using SQLite2DB.ASE.Models;
using DataAccess.ASE;
using DbEntity.ASE;
using Models.EquipmentMaintenance.ResultQuery;

namespace SQLite2DB.ASE
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
                        using (ASEDbEntities db = new ASEDbEntities())
                        {
                            var uploadLog = db.UPLOADLOG.FirstOrDefault(x => x.UNIQUEID == uploadLogUniqueID);

                            if (uploadLog != null)
                            {
                                uploadLog.TRANSTIME = DateTime.Now;

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
                        cmd.CommandText = "SELECT JobUniqueID, ArriveDate FROM ArriveRecord";

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
                                        CheckDate = x["ArriveDate"].ToString()
                                    }).Distinct().ToList();

                                    var jobUniqueIDList = temp.Where(x => uploadList.Contains(x.JobUniqueID)).Select(x => x.JobUniqueID).Distinct().ToList();

                                    using (ASEDbEntities edb = new ASEDbEntities())
                                    {
                                        foreach (var jobUniqueID in jobUniqueIDList)
                                        {
                                            var job = edb.JOB.FirstOrDefault(x => x.UNIQUEID == jobUniqueID);

                                            if (job != null)
                                            {
                                                //用同一派工的最早到位日期當作派工周期的判斷基準
                                                var jobDate = DateTimeHelper.DateString2DateTime(temp.Where(x => x.JobUniqueID == jobUniqueID).Select(x => x.CheckDate).OrderBy(x => x).First()).Value;

                                                DateTime begin, end;

                                                JobCycleHelper.GetDateSpan(jobDate, job.BEGINDATE.Value, job.ENDDATE, job.CYCLECOUNT.Value, job.CYCLEMODE, out begin, out end);

                                                var beginDateString = DateTimeHelper.DateTime2DateString(begin);
                                                var endDateString = DateTimeHelper.DateTime2DateString(end);

                                                var jobResult = edb.JOBRESULT.FirstOrDefault(x => x.JOBUNIQUEID == job.UNIQUEID && x.BEGINDATE == beginDateString && x.ENDDATE == endDateString);

                                                jobResultList.Add(new JobResultModel()
                                                {
                                                    UniqueID = jobResult != null ? jobResult.UNIQUEID : Guid.NewGuid().ToString(),
                                                    JobUniqueID = job.UNIQUEID,
                                                    BeginDate = beginDateString,
                                                    EndDate = endDateString,
                                                    CurrentOverTimeReason = jobResult != null ? jobResult.OVERTIMEREASON : string.Empty,
                                                    CurrentOverTimeUser = jobResult != null ? jobResult.OVERTIMEUSER : string.Empty,
                                                    CurrentUnPatrolReason = jobResult != null ? jobResult.UNPATROLREASON : string.Empty,
                                                    CurrentUnPatrolUser = jobResult != null ? jobResult.UNPATROLUSER : string.Empty
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    var arriveRecordList = new List<ARRIVERECORD>();

                    #region ArriveRecord
                    using (ASEDbEntities db = new ASEDbEntities())
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

                                            if (!db.ARRIVERECORD.Any(x => x.UNIQUEID == uniqueID))
                                            {
                                                var jobUniqueID = dt.Rows[i]["JobUniqueID"].ToString();
                                                var controlPointUniqueID = dt.Rows[i]["ControlPointUniqueID"].ToString();

                                                var query = (from x in db.JOBCONTROLPOINT
                                                             join c in db.CONTROLPOINT
                                                             on x.CONTROLPOINTUNIQUEID equals c.UNIQUEID
                                                             join j in db.JOB
                                                             on x.JOBUNIQUEID equals j.UNIQUEID
                                                             join r in db.ROUTE
                                                             on j.ROUTEUNIQUEID equals r.UNIQUEID
                                                             where x.JOBUNIQUEID == jobUniqueID && x.CONTROLPOINTUNIQUEID == controlPointUniqueID
                                                             select new
                                                             {
                                                                 OrganizationUniqueID = r.ORGANIZATIONUNIQUEID,
                                                                 RouteUniqueID = r.UNIQUEID,
                                                                 RouteID = r.ID,
                                                                 RouteName = r.NAME,
                                                                 JobUniqueID = j.UNIQUEID,
                                                                 JobDescription = j.DESCRIPTION,
                                                                 ControlPointUniqueID = c.UNIQUEID,
                                                                 ControlPointID = c.ID,
                                                                 ControlPointDescription = c.DESCRIPTION,
                                                                 CycleCount = j.CYCLECOUNT,
                                                                 CycleMode = j.CYCLEMODE,
                                                                 BeginDate = j.BEGINDATE,
                                                                 EndDate = j.ENDDATE
                                                             }).FirstOrDefault();

                                                if (query != null)
                                                {
                                                    var unRFIDReasonUniqueID = dt.Rows[i]["UnRFIDReasonUniqueID"].ToString();
                                                    var unRFIDReason = db.UNRFIDREASON.FirstOrDefault(x => x.UNIQUEID == unRFIDReasonUniqueID);

                                                    var timeSpanAbnormalReasonUniqueID = dt.Rows[i]["TimeSpanAbnormalReasonUniqueID"].ToString();
                                                    var timeSpanAbnormalReason = db.TIMESPANABNORMALREASON.FirstOrDefault(x => x.UNIQUEID == timeSpanAbnormalReasonUniqueID);

                                                    var userID = dt.Rows[i]["UserID"].ToString();

                                                    var user = UserDataAccessor.GetUser(userID);

                                                    var arriveRecord = new ARRIVERECORD()
                                                    {
                                                        UNIQUEID = uniqueID,
                                                        JOBRESULTUNIQUEID = jobResult.UniqueID,
                                                        ORGANIZATIONUNIQUEID = query.OrganizationUniqueID,
                                                        JOBUNIQUEID = query.JobUniqueID,
                                                        JOBDESCRIPTION = query.JobDescription,
                                                        ROUTEUNIQUEID = query.RouteUniqueID,
                                                        ROUTEID = query.RouteID,
                                                        ROUTENAME = query.RouteName,
                                                        CONTROLPOINTUNIQUEID = query.ControlPointUniqueID,
                                                        CONTROLPOINTID = query.ControlPointID,
                                                        CONTROLPOINTDESCRIPTION = query.ControlPointDescription,
                                                        ARRIVEDATE = dt.Rows[i]["ArriveDate"].ToString(),
                                                        ARRIVETIME = dt.Rows[i]["ArriveTime"].ToString(),
                                                        USERID = userID,
                                                        USERNAME = user != null ? user.Name : "",
                                                        UNRFIDREASONUNIQUEID = unRFIDReasonUniqueID,
                                                        UNRFIDREASONID = unRFIDReason != null ? unRFIDReason.ID : (unRFIDReasonUniqueID == "OTHER" ? "OTHER" : ""),
                                                        UNRFIDREASONDESCRIPTION = unRFIDReason != null ? unRFIDReason.DESCRIPTION : (unRFIDReasonUniqueID == "OTHER" ? Resources.Resource.Other : ""),
                                                        UNRFIDREASONREMARK = dt.Rows[i]["UnRFIDReasonRemark"].ToString(),
                                                        MINTIMESPAN = dt.Rows[i]["MinTimeSpan"] != null && !string.IsNullOrEmpty(dt.Rows[i]["MinTimeSpan"].ToString()) ? decimal.Parse(dt.Rows[i]["MinTimeSpan"].ToString()) : default(decimal?),
                                                        TOTALTIMESPAN = dt.Rows[i]["TotalTimeSpan"] != null && !string.IsNullOrEmpty(dt.Rows[i]["TotalTimeSpan"].ToString()) ? decimal.Parse(dt.Rows[i]["TotalTimeSpan"].ToString()) : default(decimal?),
                                                        TSABNORMALREASONUNIQUEID = timeSpanAbnormalReasonUniqueID,
                                                        TIMESPANABNORMALREASONID = timeSpanAbnormalReason != null ? timeSpanAbnormalReason.ID : (timeSpanAbnormalReasonUniqueID == "OTHER" ? "OTHER" : ""),
                                                        TIMESPANABNORMALREASONDESC = timeSpanAbnormalReason != null ? timeSpanAbnormalReason.DESCRIPTION : (timeSpanAbnormalReasonUniqueID == "OTHER" ? Resources.Resource.Other : ""),
                                                        TIMESPANABNORMALREASONREMARK = dt.Rows[i]["TimeSpanAbnormalReasonRemark"].ToString()
                                                    };

                                                    db.ARRIVERECORD.Add(arriveRecord);

                                                    arriveRecordList.Add(arriveRecord);

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

                    var checkResultAbnormalList = new Dictionary<string, CHECKRESULT>();

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
                                        using (ASEDbEntities db = new ASEDbEntities())
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var uniqueID = dt.Rows[i]["UniqueID"].ToString();

                                                if (!db.CHECKRESULT.Any(x => x.UNIQUEID == uniqueID))
                                                {
                                                    var jobUniqueID = dt.Rows[i]["JobUniqueID"].ToString();
                                                    var controlPointUniqueID = dt.Rows[i]["ControlPointUniqueID"].ToString();
                                                    var equipmentUniqueID = dt.Rows[i]["EquipmentUniqueID"].ToString();
                                                    var partUniqueID = dt.Rows[i]["PartUniqueID"].ToString();
                                                    var checkItemUniqueID = dt.Rows[i]["CheckItemUniqueID"].ToString();

                                                    if (string.IsNullOrEmpty(equipmentUniqueID))
                                                    {
                                                        var query = (from x in db.JOBCONTROLPOINTCHECKITEM
                                                                     join y in db.CONTROLPOINTCHECKITEM
                                                                     on new { x.CONTROLPOINTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.CONTROLPOINTUNIQUEID, y.CHECKITEMUNIQUEID }
                                                                     join item in db.CHECKITEM
                                                                     on x.CHECKITEMUNIQUEID equals item.UNIQUEID
                                                                     join c in db.CONTROLPOINT
                                                                     on x.CONTROLPOINTUNIQUEID equals c.UNIQUEID
                                                                     join j in db.JOB
                                                                     on x.JOBUNIQUEID equals j.UNIQUEID
                                                                     join r in db.ROUTE
                                                                     on j.ROUTEUNIQUEID equals r.UNIQUEID
                                                                     where x.JOBUNIQUEID == jobUniqueID && x.CONTROLPOINTUNIQUEID == controlPointUniqueID && x.CHECKITEMUNIQUEID == checkItemUniqueID
                                                                     select new
                                                                     {
                                                                         OrganizationUniqueID = r.ORGANIZATIONUNIQUEID,
                                                                         RouteUniqueID = r.UNIQUEID,
                                                                         RouteID = r.ID,
                                                                         RouteName = r.NAME,
                                                                         JobUniqueID = j.UNIQUEID,
                                                                         JobDescription = j.DESCRIPTION,
                                                                         ControlPointUniqueID = c.UNIQUEID,
                                                                         ControlPointID = c.ID,
                                                                         ControlPointDescription = c.DESCRIPTION,
                                                                         CheckItemUniqueID = y.CHECKITEMUNIQUEID,
                                                                         CheckItemID = item.ID,
                                                                         CheckItemDescription = item.DESCRIPTION,
                                                                         LowerLimit =y.ISINHERIT=="Y"? item.LOWERLIMIT:y.LOWERLIMIT,
                                                                         LowerAlertLimit = y.ISINHERIT == "Y" ? item.LOWERALERTLIMIT : y.LOWERALERTLIMIT,
                                                                         UpperAlertLimit = y.ISINHERIT == "Y" ? item.UPPERALERTLIMIT : y.UPPERALERTLIMIT,
                                                                         UpperLimit = y.ISINHERIT == "Y" ? item.UPPERLIMIT : y.UPPERLIMIT,
                                                                         AccumulationBase = y.ISINHERIT == "Y" ? item.ACCUMULATIONBASE : y.ACCUMULATIONBASE,
                                                                         IsAccumulation = item.ISACCUMULATION=="Y",
                                                                         Unit = y.ISINHERIT == "Y" ? item.UNIT : y.UNIT
                                                                     }).FirstOrDefault();

                                                        if (query != null)
                                                        {
                                                            var checkResult = new CHECKRESULT()
                                                            {
                                                                UNIQUEID = uniqueID,
                                                                ARRIVERECORDUNIQUEID = dt.Rows[i]["ArriveRecordUniqueID"].ToString(),
                                                                ORGANIZATIONUNIQUEID = query.OrganizationUniqueID,
                                                                JOBUNIQUEID = query.JobUniqueID,
                                                                JOBDESCRIPTION = query.JobDescription,
                                                                ROUTEUNIQUEID = query.RouteUniqueID,
                                                                ROUTEID = query.RouteID,
                                                                ROUTENAME = query.RouteName,
                                                                CONTROLPOINTUNIQUEID = query.ControlPointUniqueID,
                                                                CONTROLPOINTID = query.ControlPointID,
                                                                CONTROLPOINTDESCRIPTION = query.ControlPointDescription,
                                                                EQUIPMENTUNIQUEID = "",
                                                                EQUIPMENTID = "",
                                                                EQUIPMENTNAME = "",
                                                                PARTUNIQUEID = "",
                                                                PARTDESCRIPTION = "",
                                                                CHECKITEMUNIQUEID = query.CheckItemUniqueID,
                                                                CHECKITEMID = query.CheckItemID,
                                                                CHECKITEMDESCRIPTION = query.CheckItemDescription,
                                                                LOWERLIMIT = query.LowerLimit,
                                                                LOWERALERTLIMIT = query.LowerAlertLimit,
                                                                UPPERALERTLIMIT = query.UpperAlertLimit,
                                                                UPPERLIMIT = query.UpperLimit,
                                                                UNIT = query.Unit,
                                                                CHECKDATE = dt.Rows[i]["CheckDate"].ToString(),
                                                                CHECKTIME = dt.Rows[i]["CheckTime"].ToString(),
                                                                FEELOPTIONUNIQUEID = dt.Rows[i]["FeelOptionUniqueID"].ToString(),
                                                                ISABNORMAL = "N",
                                                                ISALERT = "N"
                                                            };

                                                            try
                                                            {
                                                                var remark = dt.Rows[i]["Remark"].ToString();

                                                                checkResult.REMARK = remark;
                                                            }
                                                            catch
                                                            { }

                                                            var otherMk = dt.Rows[i]["OtherMk"].ToString();

                                                            if (!string.IsNullOrEmpty(otherMk))
                                                            {
                                                                checkResult.OTHERMK = otherMk;

                                                                switch (otherMk)
                                                                {
                                                                    case "1":
                                                                        checkResult.OTHERMKDESCRIPTION = Resources.Resource.OtherMk1;
                                                                        break;
                                                                    case "2":
                                                                        checkResult.OTHERMKDESCRIPTION = Resources.Resource.OtherMk2;
                                                                        break;
                                                                    case "3":
                                                                        checkResult.OTHERMKDESCRIPTION = Resources.Resource.OtherMk3;
                                                                        break;
                                                                }

                                                                checkResult.RESULT = checkResult.OTHERMKDESCRIPTION;
                                                            }

                                                            var value = dt.Rows[i]["Value"].ToString();

                                                            if (!string.IsNullOrEmpty(value))
                                                            {
                                                                decimal val = decimal.Round(decimal.Parse(value), 5);
                                                                decimal netVal = val;

                                                                checkResult.VALUE = val;

                                                                if (query.IsAccumulation)
                                                                {
                                                                    var prevCheckResult = db.CHECKRESULT.Where(x => x.CONTROLPOINTUNIQUEID == query.ControlPointUniqueID && x.CHECKITEMUNIQUEID == query.CheckItemUniqueID && x.VALUE.HasValue).OrderByDescending(x => x.CHECKDATE).ThenByDescending(x => x.CHECKTIME).FirstOrDefault();

                                                                    if (prevCheckResult != null)
                                                                    {
                                                                        netVal = decimal.Round(val - prevCheckResult.VALUE.Value, 5);
                                                                        checkResult.NETVALUE = netVal;
                                                                    }
                                                                    else
                                                                    {
                                                                        netVal = decimal.Round(val - query.AccumulationBase.Value, 5);
                                                                        checkResult.NETVALUE = netVal;
                                                                    }
                                                                }

                                                                if (!string.IsNullOrEmpty(checkResult.OTHERMK))
                                                                {
                                                                    checkResult.RESULT = string.Format("{0}({1})", checkResult.OTHERMKDESCRIPTION, netVal.ToString("F2"));
                                                                }
                                                                else
                                                                {
                                                                    checkResult.RESULT = netVal.ToString("F2");
                                                                }

                                                                if (query.UpperLimit.HasValue && netVal > query.UpperLimit.Value)
                                                                {
                                                                    checkResult.ISABNORMAL = "Y";
                                                                }

                                                                if (query.UpperAlertLimit.HasValue && netVal > query.UpperAlertLimit.Value)
                                                                {
                                                                    checkResult.ISALERT = "Y";
                                                                }

                                                                if (query.LowerLimit.HasValue && netVal < query.LowerLimit.Value)
                                                                {
                                                                    checkResult.ISABNORMAL = "Y";
                                                                }

                                                                if (query.LowerAlertLimit.HasValue && netVal < query.LowerAlertLimit.Value)
                                                                {
                                                                    checkResult.ISALERT = "Y";
                                                                }
                                                            }

                                                            if (!string.IsNullOrEmpty(checkResult.FEELOPTIONUNIQUEID))
                                                            {
                                                                var feelOption = db.CHECKITEMFEELOPTION.FirstOrDefault(x => x.UNIQUEID == checkResult.FEELOPTIONUNIQUEID);

                                                                if (feelOption != null)
                                                                {
                                                                    checkResult.FEELOPTIONDESCRIPTION = feelOption.DESCRIPTION;
                                                                    checkResult.ISABNORMAL = feelOption.ISABNORMAL;

                                                                    if (!string.IsNullOrEmpty(checkResult.OTHERMK))
                                                                    {
                                                                        checkResult.RESULT = string.Format("{0}({1})", checkResult.OTHERMKDESCRIPTION, feelOption.DESCRIPTION);
                                                                    }
                                                                    else
                                                                    {
                                                                        checkResult.RESULT = feelOption.DESCRIPTION;
                                                                    }
                                                                }
                                                            }

                                                            if (checkResult.ISABNORMAL == "Y" || checkResult.ISALERT == "Y")
                                                            {
                                                                var abnormalUniqueID = Guid.NewGuid().ToString();

                                                                db.ABNORMAL.Add(new ABNORMAL()
                                                                {
                                                                    UNIQUEID = abnormalUniqueID
                                                                });

                                                                db.ABNORMALCHECKRESULT.Add(new ABNORMALCHECKRESULT()
                                                                {
                                                                    ABNORMALUNIQUEID = abnormalUniqueID,
                                                                    CHECKRESULTUNIQUEID = checkResult.UNIQUEID
                                                                });

                                                                checkResultAbnormalList.Add(abnormalUniqueID, checkResult);
                                                            }

                                                            db.CHECKRESULT.Add(checkResult);

                                                            TransCheckResultPhoto(db, conn, uniqueID, extractPath);

                                                            TransCheckResultAbnormalReason(db, conn, uniqueID);

                                                            TransCheckResultHandlingMethod(db, conn, uniqueID);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var query = (from x in db.JOBEQUIPMENTCHECKITEM
                                                                     join y in db.EQUIPMENTCHECKITEM
                                                                     on new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID, x.CHECKITEMUNIQUEID } equals new { y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID, y.CHECKITEMUNIQUEID }
                                                                     join item in db.CHECKITEM
                                                                     on x.CHECKITEMUNIQUEID equals item.UNIQUEID
                                                                     join e in db.EQUIPMENT
                                                                     on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                                     join p in db.EQUIPMENTPART
                                                                     on new { x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID } equals new { p.EQUIPMENTUNIQUEID, PARTUNIQUEID = p.UNIQUEID } into tmpPart
                                                                     from p in tmpPart.DefaultIfEmpty()
                                                                     join controlPoint in db.CONTROLPOINT
                                                                     on x.CONTROLPOINTUNIQUEID equals controlPoint.UNIQUEID
                                                                     join job in db.JOB
                                                                    on x.JOBUNIQUEID equals job.UNIQUEID
                                                                     join route in db.ROUTE
                                                                      on job.ROUTEUNIQUEID equals route.UNIQUEID
                                                                     where x.JOBUNIQUEID == jobUniqueID && x.CONTROLPOINTUNIQUEID == controlPointUniqueID && x.EQUIPMENTUNIQUEID == equipmentUniqueID && x.PARTUNIQUEID == partUniqueID && x.CHECKITEMUNIQUEID == checkItemUniqueID && uploadList.Contains(job.UNIQUEID)//UploadDefine
                                                                     select new
                                                                     {
                                                                         OrganizationUniqueID = route.ORGANIZATIONUNIQUEID,
                                                                         JobUniqueID = job.UNIQUEID,
                                                                         JobDescription = job.DESCRIPTION,
                                                                         RouteUniqueID = route.UNIQUEID,
                                                                         RouteID = route.ID,
                                                                         RouteName = route.NAME,
                                                                         ControlPointUniqueID = controlPoint.UNIQUEID,
                                                                         ControlPointID = controlPoint.ID,
                                                                         ControlPointDescription = controlPoint.DESCRIPTION,
                                                                         EquipmentUniqueID = e.UNIQUEID,
                                                                         EquipmentID = e.ID,
                                                                         EquipmentName = e.NAME,
                                                                         PartUniqueID = x.PARTUNIQUEID,
                                                                         PartDescription = p != null ? p.DESCRIPTION : "",
                                                                         CheckItemUniqueID = y.CHECKITEMUNIQUEID,
                                                                         CheckItemID = item.ID,
                                                                         CheckItemDescription = item.DESCRIPTION,
                                                                         LowerLimit = y.ISINHERIT == "Y" ? item.LOWERLIMIT : y.LOWERLIMIT,
                                                                         LowerAlertLimit = y.ISINHERIT == "Y" ? item.LOWERALERTLIMIT : y.LOWERALERTLIMIT,
                                                                         UpperAlertLimit = y.ISINHERIT == "Y" ? item.UPPERALERTLIMIT : y.UPPERALERTLIMIT,
                                                                         UpperLimit = y.ISINHERIT == "Y" ? item.UPPERLIMIT : y.UPPERLIMIT,
                                                                         AccumulationBase = y.ISINHERIT == "Y" ? item.ACCUMULATIONBASE : y.ACCUMULATIONBASE,
                                                                         IsAccumulation = item.ISACCUMULATION=="Y",
                                                                         Unit = y.ISINHERIT == "Y" ? item.UNIT : y.UNIT
                                                                     }).FirstOrDefault();

                                                        if (query != null)
                                                        {
                                                            var checkResult = new CHECKRESULT()
                                                            {
                                                                UNIQUEID = uniqueID,
                                                                ARRIVERECORDUNIQUEID = dt.Rows[i]["ArriveRecordUniqueID"].ToString(),
                                                                ORGANIZATIONUNIQUEID = query.OrganizationUniqueID,
                                                                JOBUNIQUEID = query.JobUniqueID,
                                                                JOBDESCRIPTION = query.JobDescription,
                                                                ROUTEUNIQUEID = query.RouteUniqueID,
                                                                ROUTEID = query.RouteID,
                                                                ROUTENAME = query.RouteName,
                                                                CONTROLPOINTUNIQUEID = query.ControlPointUniqueID,
                                                                CONTROLPOINTID = query.ControlPointID,
                                                                CONTROLPOINTDESCRIPTION = query.ControlPointDescription,
                                                                EQUIPMENTUNIQUEID = query.EquipmentUniqueID,
                                                                EQUIPMENTID = query.EquipmentID,
                                                                EQUIPMENTNAME = query.EquipmentName,
                                                                PARTUNIQUEID = query.PartUniqueID,
                                                                PARTDESCRIPTION = query.PartDescription,
                                                                CHECKITEMUNIQUEID = query.CheckItemUniqueID,
                                                                CHECKITEMID = query.CheckItemID,
                                                                CHECKITEMDESCRIPTION = query.CheckItemDescription,
                                                                LOWERLIMIT = query.LowerLimit,
                                                                LOWERALERTLIMIT = query.LowerAlertLimit,
                                                                UPPERALERTLIMIT = query.UpperAlertLimit,
                                                                UPPERLIMIT = query.UpperLimit,
                                                                UNIT = query.Unit,
                                                                CHECKDATE = dt.Rows[i]["CheckDate"].ToString(),
                                                                CHECKTIME = dt.Rows[i]["CheckTime"].ToString(),
                                                                FEELOPTIONUNIQUEID = dt.Rows[i]["FeelOptionUniqueID"].ToString(),
                                                                ISABNORMAL = "N"
                                                            };

                                                            var otherMk = dt.Rows[i]["OtherMk"].ToString();

                                                            if (!string.IsNullOrEmpty(otherMk))
                                                            {
                                                                checkResult.OTHERMK = otherMk;

                                                                switch (otherMk)
                                                                {
                                                                    case "1":
                                                                        checkResult.OTHERMKDESCRIPTION = Resources.Resource.OtherMk1;
                                                                        break;
                                                                    case "2":
                                                                        checkResult.OTHERMKDESCRIPTION = Resources.Resource.OtherMk2;
                                                                        break;
                                                                    case "3":
                                                                        checkResult.OTHERMKDESCRIPTION = Resources.Resource.OtherMk3;
                                                                        break;
                                                                }

                                                                checkResult.RESULT = checkResult.OTHERMKDESCRIPTION;
                                                            }

                                                            var value = dt.Rows[i]["Value"].ToString();

                                                            if (!string.IsNullOrEmpty(value))
                                                            {
                                                                decimal val = decimal.Round(decimal.Parse(value), 5);
                                                                decimal netVal = val;

                                                                checkResult.VALUE = val;

                                                                if (query.IsAccumulation)
                                                                {
                                                                    var prevCheckResult = db.CHECKRESULT.Where(x => x.EQUIPMENTUNIQUEID == query.EquipmentUniqueID && query.PartUniqueID == query.PartUniqueID && x.CHECKITEMUNIQUEID == query.CheckItemUniqueID && x.VALUE.HasValue).OrderByDescending(x => x.CHECKDATE).ThenByDescending(x => x.CHECKTIME).FirstOrDefault();

                                                                    if (prevCheckResult != null)
                                                                    {
                                                                        netVal = decimal.Round(val - prevCheckResult.VALUE.Value, 5);
                                                                        checkResult.NETVALUE = netVal;
                                                                    }
                                                                    else
                                                                    {
                                                                        netVal = decimal.Round(val - query.AccumulationBase.Value, 5);
                                                                        checkResult.NETVALUE = netVal;
                                                                    }
                                                                }

                                                                if (!string.IsNullOrEmpty(checkResult.OTHERMK))
                                                                {
                                                                    checkResult.RESULT = string.Format("{0}({1})", checkResult.OTHERMKDESCRIPTION, netVal.ToString("F2"));
                                                                }
                                                                else
                                                                {
                                                                    checkResult.RESULT = netVal.ToString("F2");
                                                                }

                                                                if (query.UpperLimit.HasValue && netVal > query.UpperLimit.Value)
                                                                {
                                                                    checkResult.ISABNORMAL = "Y";
                                                                }

                                                                if (query.UpperAlertLimit.HasValue && netVal > query.UpperAlertLimit.Value)
                                                                {
                                                                    checkResult.ISALERT = "Y";
                                                                }

                                                                if (query.LowerLimit.HasValue && netVal < query.LowerLimit.Value)
                                                                {
                                                                    checkResult.ISABNORMAL = "Y";
                                                                }

                                                                if (query.LowerAlertLimit.HasValue && netVal < query.LowerAlertLimit.Value)
                                                                {
                                                                    checkResult.ISALERT = "Y";
                                                                }
                                                            }

                                                            if (!string.IsNullOrEmpty(checkResult.FEELOPTIONUNIQUEID))
                                                            {
                                                                var feelOption = db.CHECKITEMFEELOPTION.FirstOrDefault(x => x.UNIQUEID == checkResult.FEELOPTIONUNIQUEID);

                                                                if (feelOption != null)
                                                                {
                                                                    checkResult.FEELOPTIONDESCRIPTION = feelOption.DESCRIPTION;
                                                                    checkResult.ISABNORMAL = feelOption.ISABNORMAL;

                                                                    if (!string.IsNullOrEmpty(checkResult.OTHERMK))
                                                                    {
                                                                        checkResult.RESULT = string.Format("{0}({1})", checkResult.OTHERMKDESCRIPTION, feelOption.DESCRIPTION);
                                                                    }
                                                                    else
                                                                    {
                                                                        checkResult.RESULT = feelOption.DESCRIPTION;
                                                                    }
                                                                }
                                                            }

                                                            if (checkResult.ISABNORMAL=="Y"||checkResult.ISALERT=="Y")
                                                            {
                                                                var abnormalUniqueID = Guid.NewGuid().ToString();

                                                                db.ABNORMAL.Add(new ABNORMAL()
                                                                {
                                                                    UNIQUEID = abnormalUniqueID
                                                                });

                                                                db.ABNORMALCHECKRESULT.Add(new ABNORMALCHECKRESULT()
                                                                {
                                                                    ABNORMALUNIQUEID = abnormalUniqueID,
                                                                    CHECKRESULTUNIQUEID = checkResult.UNIQUEID
                                                                });

                                                                checkResultAbnormalList.Add(abnormalUniqueID, checkResult);
                                                            }

                                                            db.CHECKRESULT.Add(checkResult);

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

                    var abnormalControlPointList = arriveRecordList.Where(x => !string.IsNullOrEmpty(x.UNRFIDREASONUNIQUEID)).Select(x => new { x.JOBUNIQUEID, x.CONTROLPOINTUNIQUEID }).ToList();

                    var arriveRecordAbnormalList = new List<ARRIVERECORD>();

                    foreach (var abnormalControlPoint in abnormalControlPointList)
                    {
                        arriveRecordAbnormalList.AddRange(arriveRecordList.Where(x => x.JOBUNIQUEID == abnormalControlPoint.JOBUNIQUEID && x.CONTROLPOINTUNIQUEID == abnormalControlPoint.CONTROLPOINTUNIQUEID).ToList());
                    }

                    if ((arriveRecordAbnormalList.Count>0|| checkResultAbnormalList.Count > 0) && Config.HaveMailSetting)
                    {
                        SendAbnormalMail(arriveRecordAbnormalList, checkResultAbnormalList);
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
                                        using (ASEDbEntities db = new ASEDbEntities())
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var overTimeReasonUniqueID = dt.Rows[i]["OverTimeReasonUniqueID"].ToString();
                                                var userID = dt.Rows[i]["UserID"].ToString();

                                                var abnormalReason = db.OVERTIMEREASON.FirstOrDefault(x => x.UNIQUEID == overTimeReasonUniqueID);

                                                jobResult.NewOverTimeReasonDescription = abnormalReason != null ? abnormalReason.DESCRIPTION : (overTimeReasonUniqueID == Define.OTHER ? Resources.Resource.Other : string.Empty);
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
                                            using (ASEDbEntities db = new ASEDbEntities())
                                            {
                                                for (int i = 0; i < dt.Rows.Count; i++)
                                                {
                                                    var unPatrolReasonUniqueID = dt.Rows[i]["UnPatrolReasonUniqueID"].ToString();
                                                    var userID = dt.Rows[i]["UserID"].ToString();

                                                    var abnormalReason = db.UNPATROLREASON.FirstOrDefault(x => x.UNIQUEID == unPatrolReasonUniqueID);

                                                    jobResult.NewUnPatrolReasonDescription = abnormalReason != null ? abnormalReason.DESCRIPTION : (unPatrolReasonUniqueID == Define.OTHER ? Resources.Resource.Other : string.Empty);
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
                                        using (ASEDbEntities db = new ASEDbEntities())
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var form = db.MFORM.FirstOrDefault(x => x.UNIQUEID == mformUniqueID);

                                                if (form != null)
                                                {
                                                    var userID = dt.Rows[i]["USERID"].ToString();
                                                    var date = dt.Rows[i]["MDATE"].ToString();
                                                    var time = dt.Rows[i]["MTIME"].ToString();
                                                    var remark = dt.Rows[i]["REMARK"].ToString();
                                                    var isNeedVerify = dt.Rows[i]["IsNeedVerify"].ToString();

                                                    mFormResultUser.Add(mformUniqueID, userID);

                                                    if (form.STATUS == "0")
                                                    {
                                                        form.STATUS = "1";
                                                        form.TAKEJOBTIME = DateTime.Now;
                                                        form.TAKEJOBUSERID = userID;
                                                    }

                                                    var user = db.ACCOUNT.FirstOrDefault(x => x.ID == userID);

                                                    db.MFORMRESULT.Add(new MFORMRESULT()
                                                    {
                                                        UNIQUEID = resultUniqueID,
                                                        PMDATE = date,
                                                        MFORMUNIQUEID = mformUniqueID,
                                                        PMTIME = time,
                                                        USERID = userID,
                                                        USERNAME = user != null ? user.NAME : "",
                                                        JOBREMARK = remark
                                                    });

                                                    db.SaveChanges();

                                                    if (isNeedVerify == "Y")
                                                    {
                                                        MaintenanceFormDataAccessor.Submit(mformUniqueID);
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
                                        using (ASEDbEntities db = new ASEDbEntities())
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                if (mFormResultUser.Any(x => x.Key == mformUniqueID) && dt.Rows[i]["WorkingHour"] != null && !string.IsNullOrEmpty(dt.Rows[i]["WorkingHour"].ToString()))
                                                {
                                                    var seq = 1;

                                                    var workingHours = db.MFORMWORKINGHOUR.Where(x => x.MFORMUNIQUEID == mformUniqueID).ToList();

                                                    if (workingHours.Count > 0)
                                                    {
                                                        seq = workingHours.Max(x => x.SEQ) + 1;
                                                    }

                                                    db.MFORMWORKINGHOUR.Add(new MFORMWORKINGHOUR()
                                                    {
                                                        MFORMUNIQUEID = mformUniqueID,
                                                        SEQ = seq,
                                                        USERID = mFormResultUser[mformUniqueID],
                                                        BEGINDATE = dt.Rows[i]["BeginDate"].ToString(),
                                                        ENDDATE = dt.Rows[i]["EndDate"].ToString(),
                                                        WORKINGHOUR = decimal.Parse(dt.Rows[i]["WorkingHour"].ToString())
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
                                        using (ASEDbEntities db = new ASEDbEntities())
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var mFormUniqueID = dt.Rows[i]["MFormUniqueID"].ToString();
                                                var materialUniqueID = dt.Rows[i]["MaterialUniqueID"].ToString();
                                                var qty = int.Parse(dt.Rows[i]["Quantity"].ToString());

                                                var materialResult = db.MFORMMATERIALRESULT.FirstOrDefault(x => x.MFORMUNIQUEID == mFormUniqueID && x.MATERIALUNIQUEID == materialUniqueID);

                                                if (materialResult == null)
                                                {
                                                    var query = (from f in db.MFORM
                                                                 join e in db.EQUIPMENT
                                                                 on f.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                                 join p in db.EQUIPMENTPART
                                                                 on f.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                                                 from p in tmpPart.DefaultIfEmpty()
                                                                 join x in db.MJOBEQUIPMENTMATERIAL
                                                                 on new { f.MJOBUNIQUEID, f.EQUIPMENTUNIQUEID, f.PARTUNIQUEID } equals new { x.MJOBUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID }
                                                                 join m in db.MATERIAL
                                                                 on x.MATERIALUNIQUEID equals m.UNIQUEID
                                                                 where f.UNIQUEID == mformUniqueID && x.MATERIALUNIQUEID == materialUniqueID
                                                                 select new
                                                                 {
                                                                     EquipmentUniqueID = f.EQUIPMENTUNIQUEID,
                                                                     EquipmentID = e.ID,
                                                                     EquipmentName = e.NAME,
                                                                     PartUniqueID = f.PARTUNIQUEID,
                                                                     PartDescription = p != null ? p.DESCRIPTION : "",
                                                                     MaterialUniqueID = x.MATERIALUNIQUEID,
                                                                     MaterialID = m.ID,
                                                                     MaterialName = m.NAME,
                                                                     Quantity = x.QUANTITY
                                                                 }).First();

                                                    db.MFORMMATERIALRESULT.Add(new MFORMMATERIALRESULT()
                                                    {
                                                        MFORMUNIQUEID = mFormUniqueID,
                                                        EQUIPMENTUNIQUEID = query.EquipmentUniqueID,
                                                        EQUIPMENTID = query.EquipmentID,
                                                        EQUIPMENTNAME = query.EquipmentName,
                                                        MATERIALUNIQUEID = materialUniqueID,
                                                        MATERIALID = query.MaterialID,
                                                        MATERIALNAME = query.MaterialName,
                                                        PARTUNIQUEID = query.PartUniqueID,
                                                        PARTDESCRIPTION = query.PartDescription,
                                                        QUANTITY = query.Quantity,
                                                        CHANGEQUANTITY = qty
                                                    });
                                                }
                                                else
                                                {
                                                    materialResult.CHANGEQUANTITY = qty;
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
                                        using (ASEDbEntities db = new ASEDbEntities())
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var standardUniqueID = dt.Rows[i]["StandardUniqueID"].ToString();

                                                var query = (from x in db.MFORM
                                                             join j in db.MJOB
                                                             on x.MJOBUNIQUEID equals j.UNIQUEID
                                                             join e in db.EQUIPMENT
                                                             on x.EQUIPMENTUNIQUEID equals e.UNIQUEID
                                                             join p in db.EQUIPMENTPART
                                                             on x.PARTUNIQUEID equals p.UNIQUEID into tmpPart
                                                             from p in tmpPart.DefaultIfEmpty()
                                                             join y in db.MJOBEQUIPMENTSTANDARD
                                                             on new { x.MJOBUNIQUEID, x.EQUIPMENTUNIQUEID, x.PARTUNIQUEID } equals new { y.MJOBUNIQUEID, y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID }
                                                             join z in db.EQUIPMENTSTANDARD
                                                             on new {y.EQUIPMENTUNIQUEID, y.PARTUNIQUEID} equals new{z.EQUIPMENTUNIQUEID, z.PARTUNIQUEID}
                                                             join s in db.STANDARD
                                                             on y.STANDARDUNIQUEID equals s.UNIQUEID
                                                             where x.UNIQUEID == mformUniqueID && s.UNIQUEID == standardUniqueID
                                                             select new
                                                             {
                                                                 MJobUniqueID = j.UNIQUEID,
                                                                 MJobDescription = j.DESCRIPTION,
                                                                 OrganizationUniqueID = j.ORGANIZATIONUNIQUEID,
                                                                 EquipmentUniqueID = x.EQUIPMENTUNIQUEID,
                                                                 EquipmentID = e.ID,
                                                                 EquipmentName = e.NAME,
                                                                 PartUniqueID = x.PARTUNIQUEID,
                                                                 PartDescription = p!=null?p.DESCRIPTION:"",
                                                                 StandardUniqueID=s.UNIQUEID,
                                                                 StandardID = s.ID,
                                                                 StandardDescription = s.DESCRIPTION,
                                                                 LowerLimit = z.ISINHERIT == "Y" ? s.LOWERLIMIT : z.LOWERLIMIT,
                                                                 LowerAlertLimit = z.ISINHERIT == "Y" ? s.LOWERALERTLIMIT : z.LOWERALERTLIMIT,
                                                                 UpperAlertLimit = z.ISINHERIT == "Y" ? s.UPPERALERTLIMIT : z.UPPERALERTLIMIT,
                                                                 UpperLimit = z.ISINHERIT == "Y" ? s.UPPERLIMIT : z.UPPERLIMIT,
                                                                 Unit = z.ISINHERIT=="Y"?s.UNIT:z.UNIT,
                                                                 AccumulationBase = z.ISINHERIT == "Y" ? s.ACCUMULATIONBASE : z.ACCUMULATIONBASE,
                                                                 IsAccumulation = s.ISACCUMULATION == "Y",
                                                             }).FirstOrDefault();

                                                if (query != null)
                                                {
                                                    var standardResult = new MFORMSTANDARDRESULT()
                                                    {
                                                        UNIQUEID = Guid.NewGuid().ToString(),
                                                        RESULTUNIQUEID = resultUniqueID,
                                                        MFORMUNIQUEID = mformUniqueID,
                                                        ORGANIZATIONUNIQUEID = query.OrganizationUniqueID,
                                                        MJOBUNIQUEID=query.MJobUniqueID,
                                                        MJOBDESCRIPTION = query.MJobDescription,
                                                        EQUIPMENTUNIQUEID = query.EquipmentUniqueID,
                                                        EQUIPMENTID = query.EquipmentID,
                                                        EQUIPMENTNAME = query.EquipmentName,
                                                        PARTUNIQUEID = query.PartUniqueID,
                                                        PARTDESCRIPTION = query.PartDescription,
                                                        STANDARDUNIQUEID = query.StandardUniqueID,
                                                        STANDARDID = query.StandardID,
                                                        STANDARDDESCRIPTION = query.StandardDescription,
                                                        FEELOPTIONUNIQUEID = dt.Rows[i]["FeelOptionUniqueID"].ToString(),
                                                        ISABNORMAL = "N",
                                                        ISALERT = "N",
                                                        LOWERALERTLIMIT = query.LowerAlertLimit,
                                                        LOWERLIMIT = query.LowerLimit,
                                                         UNIT = query.Unit,
                                                          UPPERALERTLIMIT = query.UpperAlertLimit,
                                                          UPPERLIMIT = query.UpperLimit
                                                    };

                                                    var value = dt.Rows[i]["Value"].ToString();

                                                    if (!string.IsNullOrEmpty(value))
                                                    {
                                                        decimal val = decimal.Round(decimal.Parse(value), 5);
                                                        decimal netVal = val;

                                                        standardResult.VALUE = val;

                                                        if (query.IsAccumulation)
                                                        {
                                                            var prevStandardResult = (from x in db.MFORMSTANDARDRESULT
                                                                                      join r in db.MFORMRESULT
                                                                                      on x.RESULTUNIQUEID equals r.UNIQUEID
                                                                                      where x.EQUIPMENTUNIQUEID == query.EquipmentUniqueID && x.PARTUNIQUEID == query.PartUniqueID && x.STANDARDUNIQUEID == standardUniqueID && x.VALUE.HasValue
                                                                                      select new{r.PMDATE, r.PMTIME, x.VALUE}).OrderByDescending(x=>x.PMDATE).ThenByDescending(x=>x.PMTIME).FirstOrDefault();

                                                            if (prevStandardResult != null)
                                                            {
                                                                netVal = decimal.Round(val - prevStandardResult.VALUE.Value, 5);
                                                                standardResult.NETVALUE = netVal;
                                                            }
                                                            else
                                                            {
                                                                netVal = decimal.Round(val - query.AccumulationBase.Value, 5);
                                                                standardResult.NETVALUE = netVal;
                                                            }
                                                        }

                                                        standardResult.RESULT = netVal.ToString("F2");

                                                        if (query.UpperLimit.HasValue && netVal > query.UpperLimit.Value)
                                                        {
                                                            standardResult.ISABNORMAL = "Y";
                                                        }

                                                        if (query.UpperAlertLimit.HasValue && netVal > query.UpperAlertLimit.Value)
                                                        {
                                                            standardResult.ISALERT = "Y";
                                                        }

                                                        if (query.LowerLimit.HasValue && netVal < query.LowerLimit.Value)
                                                        {
                                                            standardResult.ISABNORMAL = "Y";
                                                        }

                                                        if (query.LowerAlertLimit.HasValue && netVal < query.LowerAlertLimit.Value)
                                                        {
                                                            standardResult.ISALERT = "Y";
                                                        }
                                                    }

                                                    if (!string.IsNullOrEmpty(standardResult.FEELOPTIONUNIQUEID))
                                                    {
                                                        var feelOption = db.STANDARDFEELOPTION.FirstOrDefault(x => x.UNIQUEID == standardResult.FEELOPTIONUNIQUEID);

                                                        if (feelOption != null)
                                                        {
                                                            standardResult.ISABNORMAL = feelOption.ISABNORMAL;

                                                            standardResult.RESULT = feelOption.DESCRIPTION;
                                                        }
                                                    }

                                                    db.MFORMSTANDARDRESULT.Add(standardResult);

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

        private void TransArriveRecordPhoto(ASEDbEntities DB, SQLiteConnection Conn, string ArriveRecordUniqueID, string ExtractPath)
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

                                    DB.ARRIVERECORDPHOTO.Add(new ARRIVERECORDPHOTO()
                                    {
                                        ARRIVERECORDUNIQUEID = ArriveRecordUniqueID,
                                        SEQ = i + 1,
                                        EXTENSION = extension
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TransCheckResultPhoto(ASEDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID, string ExtractPath)
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
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                var photo = Path.Combine(ExtractPath, dt.Rows[j]["FileName"].ToString());

                                if (System.IO.File.Exists(photo))
                                {
                                    var extension = new FileInfo(photo).Extension.Substring(1);

                                    System.IO.File.Copy(photo, Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, CheckResultUniqueID + "_" + (j + 1).ToString() + "." + extension), true);

                                    DB.CHECKRESULTPHOTO.Add(new CHECKRESULTPHOTO()
                                    {
                                         CHECKRESULTUNIQUEID = CheckResultUniqueID,
                                        SEQ = j + 1,
                                        EXTENSION = extension
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TransCheckResultAbnormalReason(ASEDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID)
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

                                var abnormalReason = DB.ABNORMALREASON.FirstOrDefault(x => x.UNIQUEID == abnormalReasonUniqueID);

                                DB.CHECKRESULTABNORMALREASON.Add(new CHECKRESULTABNORMALREASON()
                                {
                                     CHECKRESULTUNIQUEID = CheckResultUniqueID,
                                    ABNORMALREASONUNIQUEID = abnormalReasonUniqueID,
                                    ABNORMALREASONID = abnormalReason != null ? abnormalReason.ID : (abnormalReasonUniqueID == "OTHER" ? "OTHER" : ""),
                                    ABNORMALREASONDESCRIPTION = abnormalReason != null ? abnormalReason.DESCRIPTION : (abnormalReasonUniqueID == "OTHER" ? Resources.Resource.Other : ""),
                                    ABNORMALREASONREMARK = dt.Rows[j]["AbnormalReasonRemark"].ToString()
                                });
                            }
                        }
                    }
                }
            }
        }

        private void TransCheckResultHandlingMethod(ASEDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID)
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

                                var handlingMethod = DB.HANDLINGMETHOD.FirstOrDefault(x => x.UNIQUEID == handlingMethodUniqueID);

                                DB.CHECKRESULTHANDLINGMETHOD.Add(new CHECKRESULTHANDLINGMETHOD()
                                {
                                     CHECKRESULTUNIQUEID = CheckResultUniqueID,
                                    ABNORMALREASONUNIQUEID = handlingMethodDt.Rows[j]["AbnormalReasonUniqueID"].ToString(),
                                    HANDLINGMETHODUNIQUEID = handlingMethodUniqueID,
                                    HANDLINGMETHODID = handlingMethod != null ? handlingMethod.ID : (handlingMethodUniqueID == "OTHER" ? "OTHER" : ""),
                                    HANDLINGMETHODDESCRIPTION = handlingMethod != null ? handlingMethod.DESCRIPTION : (handlingMethodUniqueID == "OTHER" ? Resources.Resource.Other : ""),
                                    HANDLINGMETHODREMARK = handlingMethodDt.Rows[j]["HandlingMethodRemark"].ToString()
                                });
                            }
                        }
                    }
                }
            }
        }

        private void SendAbnormalMail(List<ARRIVERECORD> AbnormalArriveRecordList, Dictionary<string, CHECKRESULT> AbnormalCheckResultList)
        {
            try
            {
                var routeJobList = AbnormalArriveRecordList.Select(x => new { x.ORGANIZATIONUNIQUEID, x.ROUTEUNIQUEID, x.JOBUNIQUEID }).ToList().Union(AbnormalCheckResultList.Values.Select(x => new { x.ORGANIZATIONUNIQUEID, x.ROUTEUNIQUEID, x.JOBUNIQUEID }).ToList()).Distinct().ToList();

                foreach (var routeJob in routeJobList)
                {
                    try
                    {
                        var mailAddressList = new List<MailAddress>();

                        using (ASEDbEntities db = new ASEDbEntities())
                        {
                            var managerList = db.ROUTEMANAGER.Where(x => x.ROUTEUNIQUEID == routeJob.ROUTEUNIQUEID).Select(x => x.USERID).ToList();

                            var organization = db.ORGANIZATION.FirstOrDefault(x => x.UNIQUEID == routeJob.ORGANIZATIONUNIQUEID);

                            if (organization != null)
                            {
                                managerList.Add(organization.MANAGERUSERID);
                            }

                            foreach (var manager in managerList)
                            {
                                var user = db.ACCOUNT.FirstOrDefault(x => x.ID == manager);

                                if (user != null && !string.IsNullOrEmpty(user.EMAIL))
                                {
                                    mailAddressList.Add(new MailAddress(user.EMAIL, user.NAME));
                                }
                            }


                            if (mailAddressList.Count > 0)
                            {
                                var temp = AbnormalArriveRecordList.Where(x => x.JOBUNIQUEID == routeJob.JOBUNIQUEID).Select(x => new
                                {
                                    CHECKDATE = x.ARRIVEDATE,
                                    x.ROUTEID,
                                    x.ROUTENAME,
                                    x.JOBDESCRIPTION
                                }).ToList().Union(AbnormalCheckResultList.Values.Where(x => x.JOBUNIQUEID == routeJob.JOBUNIQUEID).Select(x => new { x.CHECKDATE, x.ROUTEID, x.ROUTENAME, x.JOBDESCRIPTION }).ToList()).Distinct().ToList();

                                foreach (var t0 in temp)
                                {
                                    var route = string.Format("{0}/{1}-{2}", t0.ROUTEID, t0.ROUTENAME, t0.JOBDESCRIPTION);

                                    var subject = string.Format("[{0}][{1}]{2}：{3}", Resources.Resource.CheckAbnormalNotify, t0.CHECKDATE, Resources.Resource.Route, route);

                                    var th = "<th style=\"width:150px;border:1px solid #333;padding:8px;text-align:right;color:#707070;background: #F4F4F4;\">{0}</th>";
                                    var td = "<td style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{0}</td>";
                                    var tdRowspan = "<td rowspan=\"{0}\" style=\"width:400px;border:1px solid #333;padding:8px;color:#707070;\">{1}</td>";

                                    var sb = new StringBuilder();

                                    var temp1 = AbnormalArriveRecordList.Where(x => x.JOBUNIQUEID == routeJob.JOBUNIQUEID && x.ARRIVEDATE == t0.CHECKDATE).Select(x => new
                                    {
                                        x.UNIQUEID,
                                        x.CONTROLPOINTUNIQUEID,
                                        x.CONTROLPOINTID,
                                        x.CONTROLPOINTDESCRIPTION,
                                        x.ARRIVEDATE,
                                        x.ARRIVETIME,
                                        x.UNRFIDREASONID,
                                        x.UNRFIDREASONDESCRIPTION,
                                        x.UNRFIDREASONREMARK
                                    }).OrderBy(x => x.CONTROLPOINTID).ThenBy(x => x.ARRIVEDATE).ThenBy(x => x.ARRIVETIME).ToList();

                                    if (temp1.Count > 0)
                                    {
                                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                                        sb.Append("<tr>");
                                        sb.Append("<th style=\"border:1px solid #333;padding:8px;text-align:center;color:#707070;background: #F4F4F4;\" colspan=\"4\">未感應RFID異常</th>");
                                        sb.Append("</tr>");

                                        sb.Append("<tr>");

                                        sb.Append(string.Format(th, Resources.Resource.ControlPoint));
                                        sb.Append(string.Format(th, Resources.Resource.ArriveDate));
                                        sb.Append(string.Format(th, Resources.Resource.ArriveTime));
                                        sb.Append(string.Format(th, Resources.Resource.UnRFIDReason));

                                        sb.Append("</tr>");

                                        var controlPointList = temp1.Select(x => new { x.CONTROLPOINTUNIQUEID, x.CONTROLPOINTID, x.CONTROLPOINTDESCRIPTION }).Distinct().ToList();

                                        foreach (var controlPoint in controlPointList)
                                        {
                                            var controlPointArriveRecordList = temp1.Where(x => x.CONTROLPOINTUNIQUEID == controlPoint.CONTROLPOINTUNIQUEID).OrderBy(x => x.ARRIVEDATE).ThenBy(x => x.ARRIVETIME).ToList();

                                            sb.Append("<tr>");

                                            sb.Append(string.Format(tdRowspan,controlPointArriveRecordList.Count,  string.Format("{0}/{1}", controlPoint.CONTROLPOINTID, controlPoint.CONTROLPOINTDESCRIPTION)));
                                            sb.Append(string.Format(td, DateTimeHelper.DateString2DateStringWithSeparator(controlPointArriveRecordList[0].ARRIVEDATE)));
                                            sb.Append(string.Format(td, DateTimeHelper.TimeString2TimeStringWithSeperator(controlPointArriveRecordList[0].ARRIVETIME)));

                                            if (controlPointArriveRecordList[0].UNRFIDREASONID == Define.OTHER)
                                            {
                                                sb.Append(string.Format(td, controlPointArriveRecordList[0].UNRFIDREASONREMARK));
                                            }
                                            else
                                            {
                                                sb.Append(string.Format(td, controlPointArriveRecordList[0].UNRFIDREASONDESCRIPTION));
                                            }

                                            sb.Append("</tr>");

                                            for (int i = 1; i < controlPointArriveRecordList.Count; i++)
                                            {
                                                sb.Append("<tr>");

                                                sb.Append(string.Format(td, DateTimeHelper.DateString2DateStringWithSeparator(controlPointArriveRecordList[i].ARRIVEDATE)));
                                                sb.Append(string.Format(td,  DateTimeHelper.TimeString2TimeStringWithSeperator(controlPointArriveRecordList[i].ARRIVETIME)));

                                                if (controlPointArriveRecordList[i].UNRFIDREASONID == Define.OTHER)
                                                {
                                                    sb.Append(string.Format(td, controlPointArriveRecordList[i].UNRFIDREASONREMARK));
                                                }
                                                else
                                                {
                                                    sb.Append(string.Format(td, controlPointArriveRecordList[i].UNRFIDREASONDESCRIPTION));
                                                }

                                                sb.Append("</tr>");
                                            }
                                        }

                                        sb.Append("</table>");
                                    }

                                    var temp2 = AbnormalCheckResultList.Values.Where(x => x.JOBUNIQUEID == routeJob.JOBUNIQUEID && x.CHECKDATE == t0.CHECKDATE).Select(x => new
                                    {
                                        x.UNIQUEID,
                                        x.CONTROLPOINTID,
                                        x.CONTROLPOINTDESCRIPTION,
                                        x.EQUIPMENTID,
                                        x.EQUIPMENTNAME,
                                        x.PARTDESCRIPTION,
                                        x.CHECKITEMID,
                                        x.CHECKITEMDESCRIPTION,
                                        x.RESULT,
                                        x.ISABNORMAL,
                                        x.ISALERT,
                                        x.CHECKTIME,
                                        x.LOWERLIMIT,
                                        x.LOWERALERTLIMIT,
                                        x.UPPERLIMIT,
                                        x.UPPERALERTLIMIT,
                                        x.UNIT
                                    }).ToList();

                                    if (temp2.Count > 0)
                                    {
                                        sb.Append("<table style=\"1px solid #ddd;font-size:13px;border-collapse:collapse;\">");

                                        sb.Append("<tr>");
                                        sb.Append("<th style=\"border:1px solid #333;padding:8px;text-align:center;color:#707070;background: #F4F4F4;\" colspan=\"12\">檢查結果異常</th>");
                                        sb.Append("</tr>");

                                        sb.Append("<tr>");

                                        sb.Append(string.Format(th, Resources.Resource.Detail));
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

                                        foreach (var t2 in temp2)
                                        {
                                            var equipment = string.Empty;

                                            if (!string.IsNullOrEmpty(t2.EQUIPMENTNAME))
                                            {
                                                if (!string.IsNullOrEmpty(t2.PARTDESCRIPTION))
                                                {
                                                    equipment = string.Format("{0}/{1}-{2}", t2.EQUIPMENTID, t2.EQUIPMENTNAME, t2.PARTDESCRIPTION);
                                                }
                                                else
                                                {
                                                    equipment = string.Format("{0}/{1}", t2.EQUIPMENTID, t2.EQUIPMENTNAME);
                                                }
                                            }
                                            else
                                            {
                                                equipment = t2.EQUIPMENTID;
                                            }

                                            sb.Append("<tr>");

                                            var ab = AbnormalCheckResultList.FirstOrDefault(x => x.Value.UNIQUEID == t2.UNIQUEID);

                                            sb.Append(string.Format(td, string.Format("<a href=\"http://ASECL-eINSPRD01/FEM.Portal/Home/Index?ReturnUrl=http://ASECL-eINSPRD01/FEM/zh-tw/EquipmentMaintenance/AbnormalHandling/Index?UniqueID={0}\">連結</a>", ab.Key)));
                                            sb.Append(string.Format(td, string.Format("{0}/{1}", t2.CONTROLPOINTID, t2.CONTROLPOINTDESCRIPTION)));
                                            sb.Append(string.Format(td, equipment));
                                            sb.Append(string.Format(td, string.Format("{0}/{1}", t2.CHECKITEMID, t2.CHECKITEMDESCRIPTION)));
                                            sb.Append(string.Format(td, t2.CHECKTIME));
                                            sb.Append(string.Format(td, string.Format("{0}({1})", t2.RESULT, t2.ISABNORMAL == "Y" ? Resources.Resource.Abnormal : (t2.ISALERT == "Y" ? Resources.Resource.Warning : string.Empty))));
                                            sb.Append(string.Format(td, t2.UNIT));
                                            sb.Append(string.Format(td, t2.LOWERLIMIT));
                                            sb.Append(string.Format(td, t2.LOWERALERTLIMIT));
                                            sb.Append(string.Format(td, t2.UPPERALERTLIMIT));
                                            sb.Append(string.Format(td, t2.UPPERLIMIT));

                                            var abnormalReasonList = db.CHECKRESULTABNORMALREASON.Where(a => a.CHECKRESULTUNIQUEID == t2.UNIQUEID).OrderBy(a => a.ABNORMALREASONID).Select(a => new AbnormalReasonModel
                                            {
                                                Description = a.ABNORMALREASONDESCRIPTION,
                                                Remark = a.ABNORMALREASONREMARK,
                                                HandlingMethodList = db.CHECKRESULTHANDLINGMETHOD.Where(h => h.CHECKRESULTUNIQUEID == t2.UNIQUEID && h.ABNORMALREASONUNIQUEID == a.ABNORMALREASONUNIQUEID).OrderBy(h => h.HANDLINGMETHODID).Select(h => new HandlingMethodModel
                                                {
                                                    Description = h.HANDLINGMETHODDESCRIPTION,
                                                    Remark = h.HANDLINGMETHODREMARK
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
                                    }

                                    MailHelper.SendMail(mailAddressList, subject, sb.ToString());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(MethodBase.GetCurrentMethod(), string.Format("JobUniqueID:{0}", routeJob.JOBUNIQUEID));
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
