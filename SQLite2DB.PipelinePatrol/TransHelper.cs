using DbEntity.MSSQL;
using DbEntity.MSSQL.PipelinePatrol;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace SQLite2DB.PipelinePatrol
{
    public class TransHelper : IDisposable
    {
        public void Trans()
        {
            try
            {
                var folders = Directory.GetDirectories(Config.PipelinePatrolSQLiteUploadFolderPath);

                foreach (var folder in folders)
                {
                    System.IO.File.Move(Path.Combine(folder, "PipelinePatrol.Upload.zip"), Path.Combine(Config.PipelinePatrolSQLiteProcessingFolderPath, folder.Substring(folder.LastIndexOf("\\") + 1) + ".zip"));

                    Directory.Delete(folder);
                }

                var zips = Directory.GetFiles(Config.PipelinePatrolSQLiteProcessingFolderPath);

                Logger.Log(zips.Length + " Files To Trans");

                foreach (var zip in zips)
                {
                    FileInfo fileInfo = new FileInfo(zip);

                    var uploadLogUniqueID = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(fileInfo.Extension));

                    RequestResult result = Trans(uploadLogUniqueID, fileInfo);

                    if (result.IsSuccess)
                    {
                        using (PDbEntities db = new PDbEntities())
                        {
                            db.UploadLog.First(x => x.UniqueID == uploadLogUniqueID).TransTime = DateTime.Now;

                            db.SaveChanges();
                        }

                        System.IO.File.Copy(zip, Path.Combine(Config.PipelinePatrolSQLiteBackupFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.PipelinePatrolSQLiteProcessingFolderPath, uploadLogUniqueID), true);

                        Logger.Log(zip + " Trans Success");
                    }
                    else
                    {
                        System.IO.File.Copy(zip, Path.Combine(Config.PipelinePatrolSQLiteErrorFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.PipelinePatrolSQLiteProcessingFolderPath, uploadLogUniqueID), true);

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
                var extractPath = Path.Combine(Config.PipelinePatrolSQLiteProcessingFolderPath, UploadLogUniqueID);

                using (var zip = ZipFile.Read(ZipFileInfo.FullName))
                {
                    foreach (var entry in zip)
                    {
                        entry.Extract(extractPath);
                    }
                }

                var connString = string.Format("Data Source={0};Version=3;", Path.Combine(extractPath, Define.SQLite_PipelinePatrol));

                using (SQLiteConnection conn = new SQLiteConnection(connString))
                {
                    conn.Open();

                    var jobList = new List<string>();

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
                                    jobList = dt.AsEnumerable().Select(x => x["JobUniqueID"].ToString() + x["RouteUniqueID"].ToString()).ToList();
                                }
                            }
                        }
                    }
                    #endregion

                    //var refreshParameters = new List<RefreshParameters>();

                    #region ArriveRecord
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM ArriveRecord";

                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                adapter.Fill(dt);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    using (DbEntities db = new DbEntities())
                                    {
                                        using (PDbEntities pdb = new PDbEntities())
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var uniqueID = dt.Rows[i]["UniqueID"].ToString();
                                                var jobUniqueID = dt.Rows[i]["JobUniqueID"].ToString();
                                                var routeUniqueID = dt.Rows[i]["RouteUniqueID"].ToString();

                                                if (!pdb.ArriveRecord.Any(x => x.UniqueID == uniqueID) && jobList.Contains(jobUniqueID + routeUniqueID))
                                                {
                                                    var pipePointUniqueID = dt.Rows[i]["PipePointUniqueID"].ToString();

                                                    var query = (from x in pdb.RouteCheckPoint
                                                                 join p in pdb.PipePoint
                                                                 on x.PipePointUniqueID equals p.UniqueID
                                                                 join r in pdb.Route
                                                                 on x.RouteUniqueID equals r.UniqueID
                                                                 join y in pdb.JobRoute
                                                                 on r.UniqueID equals y.RouteUniqueID
                                                                 join j in pdb.Job
                                                                 on y.JobUniqueID equals j.UniqueID
                                                                 where j.UniqueID == jobUniqueID && r.UniqueID == routeUniqueID && p.UniqueID == pipePointUniqueID
                                                                 select new
                                                                 {
                                                                     j.OrganizationUniqueID,
                                                                     JobUniqueID = j.UniqueID,
                                                                     JobID = j.ID,
                                                                     JobDescription = j.Description,
                                                                     RouteUniqueID = r.UniqueID,
                                                                     RouteID = r.ID,
                                                                     RouteName = r.Name,
                                                                     PipePointUniqueID = p.UniqueID,
                                                                     PipePointID = p.ID,
                                                                     PipePointDescription = p.Name
                                                                 }).FirstOrDefault();

                                                    if (query != null)
                                                    {
                                                        var timeSpanAbnormalReasonUniqueID = dt.Rows[i]["TimeSpanAbnormalReasonUniqueID"].ToString();
                                                        var timeSpanAbnormalReason = pdb.TimeSpanAbnormalReason.FirstOrDefault(x => x.UniqueID == timeSpanAbnormalReasonUniqueID);

                                                        var userID = dt.Rows[i]["UserID"].ToString();
                                                        var user = db.User.FirstOrDefault(x => x.ID == userID);

                                                        pdb.ArriveRecord.Add(new ArriveRecord()
                                                        {
                                                            UniqueID = uniqueID,
                                                            OrganizationUniqueID = query.OrganizationUniqueID,
                                                            JobUniqueID = query.JobUniqueID,
                                                            JobID = query.JobID,
                                                            JobDescription = query.JobDescription,
                                                            RouteUniqueID = query.RouteUniqueID,
                                                            RouteID = query.RouteID,
                                                            RouteName = query.RouteName,
                                                            PipePointUniqueID = query.PipePointUniqueID,
                                                            PipePointID = query.PipePointID,
                                                            PipePointDescription = query.PipePointDescription,
                                                            ArriveDate = dt.Rows[i]["ArriveDate"].ToString(),
                                                            ArriveTime = dt.Rows[i]["ArriveTime"].ToString(),
                                                             Remark =  dt.Rows[i]["Remark"].ToString(),
                                                             LNG = dt.Rows[i]["LNG"]!=null&&!string.IsNullOrEmpty(dt.Rows[i]["LNG"].ToString())?double.Parse(dt.Rows[i]["LNG"].ToString()):default(double?),
                                                             LAT = dt.Rows[i]["LAT"]!=null&&!string.IsNullOrEmpty(dt.Rows[i]["LAT"].ToString())?double.Parse(dt.Rows[i]["LAT"].ToString()):default(double?),
                                                            UserID = userID,
                                                            UserName = user != null ? user.Name : "",
                                                            MinTimeSpan = dt.Rows[i]["MinTimeSpan"] != null && !string.IsNullOrEmpty(dt.Rows[i]["MinTimeSpan"].ToString()) ? double.Parse(dt.Rows[i]["MinTimeSpan"].ToString()) : default(double?),
                                                            TotalTimeSpan = dt.Rows[i]["TotalTimeSpan"] != null && !string.IsNullOrEmpty(dt.Rows[i]["TotalTimeSpan"].ToString()) ? double.Parse(dt.Rows[i]["TotalTimeSpan"].ToString()) : default(double?),
                                                            TimeSpanAbnormalReasonUniqueID = timeSpanAbnormalReasonUniqueID,
                                                            TimeSpanAbnormalReasonID = timeSpanAbnormalReason != null ? timeSpanAbnormalReason.ID : (timeSpanAbnormalReasonUniqueID == "OTHER" ? "OTHER" : ""),
                                                            TimeSpanAbnormalReasonDescription = timeSpanAbnormalReason != null ? timeSpanAbnormalReason.Description : (timeSpanAbnormalReasonUniqueID == "OTHER" ? Resources.Resource.Other : ""),
                                                            TimeSpanAbnormalReasonRemark = dt.Rows[i]["TimeSpanAbnormalReasonRemark"].ToString()
                                                        });

                                                        //TransArriveRecordPhoto(pdb, conn, uniqueID, extractPath);

                                                        //refreshParameters.Add(new RefreshParameters()
                                                        //{
                                                        //    JobUniqueID = query.JobUniqueID,
                                                        //    JobBeginDate = query.BeginDate,
                                                        //    JobEndDate = query.EndDate,
                                                        //    CycleCount = query.CycleCount,
                                                        //    CycleMode = query.CycleMode,
                                                        //    CheckDateString = dt.Rows[i]["ArriveDate"].ToString()
                                                        //});
                                                    }
                                                }
                                            }

                                            pdb.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    //var abnormalList = new List<CheckResult>();

                    #region CheckResult
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM CheckResult";

                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                adapter.Fill(dt);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    using (PDbEntities db = new PDbEntities())
                                    {
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            var uniqueID = dt.Rows[i]["UniqueID"].ToString();
                                            var jobUniqueID = dt.Rows[i]["JobUniqueID"].ToString();
                                            var routeUniqueID = dt.Rows[i]["RouteUniqueID"].ToString();

                                            if (!db.CheckResult.Any(x => x.UniqueID == uniqueID)&&jobList.Contains(jobUniqueID+routeUniqueID))
                                            {
                                                var pipePointUniqueID = dt.Rows[i]["PipePointUniqueID"].ToString();
                                                var checkItemUniqueID = dt.Rows[i]["CheckItemUniqueID"].ToString();

                                                var query = (from x in db.RouteCheckPointCheckItem
                                                             join c in db.View_PipePointCheckItem
                                                             on new { x.PipePointUniqueID, x.CheckItemUniqueID } equals new { c.PipePointUniqueID, c.CheckItemUniqueID }
                                                             join p in db.PipePoint
                                                             on x.PipePointUniqueID equals p.UniqueID
                                                             join r in db.Route
                                                             on x.RouteUniqueID equals r.UniqueID
                                                             join y in db.JobRoute
                                                             on r.UniqueID equals y.RouteUniqueID
                                                             join j in db.Job
                                                             on y.JobUniqueID equals j.UniqueID
                                                             where j.UniqueID == jobUniqueID && r.UniqueID == routeUniqueID && p.UniqueID == pipePointUniqueID && c.CheckItemUniqueID == checkItemUniqueID
                                                             select new
                                                             {
                                                                 j.OrganizationUniqueID,
                                                                 JobUniqueID = j.UniqueID,
                                                                 JobID = j.ID,
                                                                 JobDescription = j.Description,
                                                                 RouteUniqueID = r.UniqueID,
                                                                 RouteID = r.ID,
                                                                 RouteName = r.Name,
                                                                 PipePointUniqueID = p.UniqueID,
                                                                 PipePointID = p.ID,
                                                                 PipePointDescription = p.Name,
                                                                 CheckItemUniqueID = c.CheckItemUniqueID,
                                                                 CheckItemID = c.ID,
                                                                 CheckItemDescription = c.Description,
                                                                 c.LowerLimit,
                                                                 c.LowerAlertLimit,
                                                                 c.UpperAlertLimit,
                                                                 c.UpperLimit,
                                                                 c.AccumulationBase,
                                                                 c.Unit
                                                             }).FirstOrDefault();

                                                    if (query != null)
                                                    {
                                                        var checkResult = new CheckResult()
                                                        {
                                                            UniqueID = uniqueID,
                                                            ArriveRecordUniqueID = dt.Rows[i]["ArriveRecordUniqueID"].ToString(),
                                                            OrganizationUniqueID = query.OrganizationUniqueID,
                                                            JobUniqueID = query.JobUniqueID,
                                                            JobID = query.JobID,
                                                            JobDescription = query.JobDescription,
                                                            RouteUniqueID = query.RouteUniqueID,
                                                            RouteID = query.RouteID,
                                                            RouteName = query.RouteName,
                                                            PipePointUniqueID = query.PipePointUniqueID,
                                                            PipePointID = query.PipePointID,
                                                            PipePointDescription = query.PipePointDescription,
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

                                                        var value = dt.Rows[i]["Value"].ToString();

                                                        if (!string.IsNullOrEmpty(value))
                                                        {
                                                            double val = double.Parse(value);
                                                            double netVal = val;

                                                            checkResult.Value = val;

                                                            if (query.AccumulationBase.HasValue)
                                                            {
                                                                var prevCheckResult = db.CheckResult.Where(x => x.PipePointUniqueID == query.PipePointUniqueID && x.CheckItemUniqueID == query.CheckItemUniqueID && x.Value.HasValue).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

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

                                                            checkResult.Result = netVal.ToString("F2");

                                                            if (query.UpperLimit.HasValue && netVal > query.UpperLimit.Value)
                                                            {
                                                                checkResult.IsAbnormal = true;
                                                            }

                                                            if (query.UpperAlertLimit.HasValue && netVal > query.UpperAlertLimit.Value)
                                                            {
                                                                checkResult.IsAlert = true;
                                                            }

                                                            if (query.LowerLimit.HasValue && netVal < query.LowerLimit.Value)
                                                            {
                                                                checkResult.IsAbnormal = true;
                                                            }

                                                            if (query.LowerAlertLimit.HasValue && netVal < query.LowerAlertLimit.Value)
                                                            {
                                                                checkResult.IsAlert = true;
                                                            }
                                                        }

                                                        if (!string.IsNullOrEmpty(checkResult.FeelOptionUniqueID))
                                                        {
                                                            var feelOption = db.CheckItemFeelOption.FirstOrDefault(x => x.UniqueID == checkResult.FeelOptionUniqueID);

                                                            if (feelOption != null)
                                                            {
                                                                checkResult.FeelOptionDescription = feelOption.Description;
                                                                checkResult.IsAbnormal = feelOption.IsAbnormal;

                                                                checkResult.Result = feelOption.Description;
                                                            }
                                                        }

                                                        //if (checkResult.IsAbnormal)
                                                        //{
                                                        //    abnormalList.Add(checkResult);
                                                        //}

                                                        db.CheckResult.Add(checkResult);

                                                        TransCheckResultPhoto(db, conn, uniqueID, extractPath);

                                                        TransCheckResultAbnormalReason(db, conn, uniqueID);

                                                        TransCheckResultHandlingMethod(db, conn, uniqueID);
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

                    #region JobUserLocus
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM UserLocus";

                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                adapter.Fill(dt);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    using (PDbEntities pdb = new PDbEntities())
                                    {
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            var jobUniqueID = dt.Rows[i]["JobUniqueID"].ToString();
                                            var routeUniqueID = dt.Rows[i]["RouteUniqueID"].ToString();

                                            if (!string.IsNullOrEmpty(jobUniqueID) && !string.IsNullOrEmpty(routeUniqueID))
                                            {
                                                var userID = dt.Rows[i]["UserID"].ToString();
                                                var date = dt.Rows[i]["Date"].ToString();
                                                var time = dt.Rows[i]["Time"].ToString();

                                                if (!pdb.JobUserLocus.Any(x => x.JobUniqueID == jobUniqueID && x.RouteUniqueID == routeUniqueID && x.UserID == userID && x.CheckDate == date && x.CheckTime == time))
                                                {
                                                    pdb.JobUserLocus.Add(new JobUserLocus()
                                                    {
                                                        JobUniqueID = jobUniqueID,
                                                        RouteUniqueID = routeUniqueID,
                                                        UserID = userID,
                                                        CheckDate = date,
                                                        CheckTime = time,
                                                        LAT = double.Parse(dt.Rows[i]["LAT"].ToString()),
                                                        LNG = double.Parse(dt.Rows[i]["LNG"].ToString())
                                                    });

                                                    pdb.SaveChanges();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    //if (Config.HaveMailSetting && abnormalList.Count > 0)
                    //{
                    //    SendAbnormalMail(abnormalList);
                    //}

                    //RefreshJobResult(refreshParameters);

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

        //private void TransArriveRecordPhoto(PDbEntities DB, SQLiteConnection Conn, string ArriveRecordUniqueID, string ExtractPath)
        //{
        //    using (SQLiteCommand cmd = Conn.CreateCommand())
        //    {
        //        cmd.CommandText = string.Format("SELECT * FROM ArriveRecordPhoto WHERE ArriveRecordUniqueID = '{0}'", ArriveRecordUniqueID);

        //        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
        //        {
        //            using (DataTable dt = new DataTable())
        //            {
        //                adapter.Fill(dt);

        //                if (dt != null && dt.Rows.Count > 0)
        //                {
        //                    for (int i = 0; i < dt.Rows.Count; i++)
        //                    {
        //                        var photo = Path.Combine(ExtractPath, dt.Rows[i]["FileName"].ToString());

        //                        if (System.IO.File.Exists(photo))
        //                        {
        //                            var extension = new FileInfo(photo).Extension.Substring(1);

        //                            System.IO.File.Copy(photo, Path.Combine(Config.PipelinePatrolPhotoFolderPath, ArriveRecordUniqueID + "_" + (i + 1).ToString() + "." + extension), true);

        //                            DB.ArriveRecordPhoto.Add(new ArriveRecordPhoto()
        //                            {
        //                                ArriveRecordUniqueID = ArriveRecordUniqueID,
        //                                Seq = i + 1,
        //                                Extension = extension
        //                            });
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        private void TransCheckResultPhoto(PDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID, string ExtractPath)
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

                                    System.IO.File.Copy(photo, Path.Combine(Config.PipelinePatrolPhotoFolderPath, CheckResultUniqueID + "_" + (j + 1).ToString() + "." + extension), true);

                                    DB.CheckResultPhoto.Add(new CheckResultPhoto()
                                    {
                                        CheckResultUniqueID = CheckResultUniqueID,
                                        Seq = j + 1,
                                        Extension = extension
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TransCheckResultAbnormalReason(PDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID)
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

        private void TransCheckResultHandlingMethod(PDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID)
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

        //private void SendAbnormalMail(List<CheckResult> AbnormalList)
        //{
        //    try
        //    {
        //        var temp1 = AbnormalList.Select(x => new { x.OrganizationUniqueID, x.RouteUniqueID, x.JobUniqueID }).Distinct().ToList();

        //        foreach (var t1 in temp1)
        //        {
        //            try
        //            {
        //                var mailAddressList = new List<MailAddress>();

        //                var managerList = new List<string>();

        //                using (EDbEntities db = new EDbEntities())
        //                {
        //                    managerList = db.RouteManager.Where(x => x.RouteUniqueID == t1.RouteUniqueID).Select(x => x.UserID).ToList();
        //                }

        //                using (DbEntities db = new DbEntities())
        //                {
        //                    var organization = db.Organization.FirstOrDefault(x => x.UniqueID == t1.OrganizationUniqueID);

        //                    if (organization != null)
        //                    {
        //                        managerList.Add(organization.ManagerUserID);
        //                    }

        //                    foreach (var manager in managerList)
        //                    {
        //                        var user = db.User.FirstOrDefault(x => x.ID == manager);

        //                        if (user != null && !string.IsNullOrEmpty(user.Email))
        //                        {
        //                            mailAddressList.Add(new MailAddress(user.Email, user.Name));
        //                        }
        //                    }
        //                }

        //                if (mailAddressList.Count > 0)
        //                {
        //                    var temp2 = AbnormalList.Where(x => x.JobUniqueID == t1.JobUniqueID).Select(x => new
        //                    {
        //                        x.CheckDate,
        //                        x.RouteID,
        //                        x.RouteName,
        //                        x.JobDescription
        //                    }).Distinct().ToList();

        //                    foreach (var t2 in temp2)
        //                    {
        //                        var route = string.Format("{0}/{1}-{2}", t2.RouteID, t2.RouteName, t2.JobDescription);

        //                        var subject = string.Format("[{0}][{1}]{2}：{3}", Resources.Resource.CheckAbnormalNotify, t2.CheckDate, Resources.Resource.Route, route);

        //                        var temp3 = AbnormalList.Where(x => x.JobUniqueID == t1.JobUniqueID && x.CheckDate == t2.CheckDate).Select(x => new
        //                        {
        //                            x.ControlPointID,
        //                            x.ControlPointDescription,
        //                            x.EquipmentID,
        //                            x.EquipmentName,
        //                            x.PartDescription,
        //                            x.CheckItemID,
        //                            x.CheckItemDescription,
        //                            x.Result
        //                        }).ToList();

        //                        StringBuilder sb = new StringBuilder();

        //                        sb.Append("<table>");

        //                        sb.Append("<tr>");
        //                        sb.Append("<th>");
        //                        sb.Append(Resources.Resource.ControlPoint);
        //                        sb.Append("</th>");
        //                        sb.Append("<th>");
        //                        sb.Append(Resources.Resource.Equipment);
        //                        sb.Append("</th>");
        //                        sb.Append("<th>");
        //                        sb.Append(Resources.Resource.CheckItem);
        //                        sb.Append("</th>");
        //                        sb.Append("<th>");
        //                        sb.Append(Resources.Resource.CheckResult);
        //                        sb.Append("</th>");
        //                        sb.Append("</tr>");

        //                        foreach (var t3 in temp3)
        //                        {
        //                            sb.Append("<tr>");

        //                            sb.Append("<td>");
        //                            sb.Append(string.Format("{0}/{1}", t3.ControlPointID, t3.ControlPointDescription));
        //                            sb.Append("</td>");

        //                            sb.Append("<td>");
        //                            if (!string.IsNullOrEmpty(t3.PartDescription))
        //                            {
        //                                sb.Append(string.Format("{0}/{1}-{2}", t3.EquipmentID, t3.EquipmentName, t3.PartDescription));
        //                            }
        //                            else
        //                            {
        //                                sb.Append(string.Format("{0}/{1}", t3.EquipmentID, t3.EquipmentName));
        //                            }
        //                            sb.Append("</td>");

        //                            sb.Append("<td>");
        //                            sb.Append(string.Format("{0}/{1}", t3.CheckItemID, t3.CheckItemDescription));
        //                            sb.Append("</td>");

        //                            sb.Append("<td>");
        //                            sb.Append(t3.Result);
        //                            sb.Append("</td>");

        //                            sb.Append("</tr>");
        //                        }

        //                        sb.Append("</table>");

        //                        MailHelper.SendMail(mailAddressList, subject, sb.ToString());
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.Log(MethodBase.GetCurrentMethod(), string.Format("JobUniqueID:{0}", t1.JobUniqueID));
        //                Logger.Log(MethodBase.GetCurrentMethod(), ex);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log(MethodBase.GetCurrentMethod(), ex);
        //    }
        //}

        //private void RefreshJobResult(List<RefreshParameters> Parameters)
        //{
        //    try
        //    {
        //        var refreshItemList = new List<RefreshItem>();

        //        var temp = Parameters.Select(x => new { x.JobUniqueID, x.JobBeginDate, x.JobEndDate, x.CycleCount, x.CycleMode, x.CheckDate }).Distinct().ToList();

        //        foreach (var t in temp)
        //        {
        //            DateTime begin, end;

        //            JobCycleHelper.GetDateSpan(t.CheckDate, t.JobBeginDate, t.JobEndDate, t.CycleCount, t.CycleMode, out begin, out end);

        //            var beginDateString = DateTimeHelper.DateTime2DateString(begin);
        //            var endDateString = DateTimeHelper.DateTime2DateString(end);

        //            var item = refreshItemList.FirstOrDefault(x => x.JobUniqueID == t.JobUniqueID && x.BeginDateString == beginDateString && x.EndDateString == endDateString);

        //            if (item == null)
        //            {
        //                refreshItemList.Add(new RefreshItem()
        //                {
        //                    JobUniqueID = t.JobUniqueID,
        //                    BeginDateString = beginDateString,
        //                    EndDateString = endDateString
        //                });
        //            }
        //        }

        //        foreach (var refreshItem in refreshItemList)
        //        {
        //            JobResultDataAccessor.Refresh(refreshItem.JobUniqueID, refreshItem.BeginDateString, refreshItem.EndDateString);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log(MethodBase.GetCurrentMethod(), ex);
        //    }
        //}

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
