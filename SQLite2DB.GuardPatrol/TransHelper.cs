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
using DbEntity.MSSQL;
using DbEntity.MSSQL.GuardPatrol;
using System.Net.Mail;
using System.Text;

namespace SQLite2DB.GuardPatrol
{
    public class TransHelper : IDisposable
    {
        public void Trans()
        {
            try
            {
                var folders = Directory.GetDirectories(Config.GuardPatrolSQLiteUploadFolderPath);

                foreach (var folder in folders)
                {
                    System.IO.File.Move(Path.Combine(folder, "GuardPatrol.Upload.zip"), Path.Combine(Config.GuardPatrolSQLiteProcessingFolderPath, folder.Substring(folder.LastIndexOf("\\") + 1) + ".zip"));

                    Directory.Delete(folder);
                }

                var zips = Directory.GetFiles(Config.GuardPatrolSQLiteProcessingFolderPath);

                Logger.Log(zips.Length + " Files To Trans");

                foreach (var zip in zips)
                {
                    FileInfo fileInfo = new FileInfo(zip);

                    var uploadLogUniqueID = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(fileInfo.Extension));

                    RequestResult result = Trans(uploadLogUniqueID, fileInfo);

                    if (result.IsSuccess)
                    {
                        using (GDbEntities db = new GDbEntities())
                        {
                            var uploadLog = db.UploadLog.FirstOrDefault(x => x.UniqueID == uploadLogUniqueID);

                            if (uploadLog != null)
                            {
                                uploadLog.TransTime = DateTime.Now;

                                db.SaveChanges();
                            }
                        }


                        System.IO.File.Copy(zip, Path.Combine(Config.GuardPatrolSQLiteBackupFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.GuardPatrolSQLiteProcessingFolderPath, uploadLogUniqueID), true);

                        Logger.Log(zip + " Trans Success");
                    }
                    else
                    {
                        System.IO.File.Copy(zip, Path.Combine(Config.GuardPatrolSQLiteErrorFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.GuardPatrolSQLiteProcessingFolderPath, uploadLogUniqueID), true);

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
                var extractPath = Path.Combine(Config.GuardPatrolSQLiteProcessingFolderPath, UploadLogUniqueID);

                using (var zip = ZipFile.Read(ZipFileInfo.FullName))
                {
                    foreach (var entry in zip)
                    {
                        entry.Extract(extractPath);
                    }
                }

                var connString = string.Format("Data Source={0};Version=3;", Path.Combine(extractPath, Define.SQLite_GuardPatrol));

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
                                    jobList = dt.AsEnumerable().Select(x => x["JobUniqueID"].ToString()).ToList();
                                }
                            }
                        }
                    }
                    #endregion

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
                                        using (GDbEntities gdb = new GDbEntities())
                                        {
                                            for (int i = 0; i < dt.Rows.Count; i++)
                                            {
                                                var uniqueID = dt.Rows[i]["UniqueID"].ToString();

                                                if (!gdb.ArriveRecord.Any(x => x.UniqueID == uniqueID))
                                                {
                                                    var jobRouteUniqueID = dt.Rows[i]["JobUniqueID"].ToString();
                                                    var controlPointUniqueID = dt.Rows[i]["ControlPointUniqueID"].ToString();

                                                    var query = (from x in gdb.JobRoute
                                                                 join j in gdb.Job
                                                                 on x.JobUniqueID equals j.UniqueID
                                                                 join r in gdb.Route
                                                                 on x.RouteUniqueID equals r.UniqueID
                                                                 join y in gdb.JobControlPoint
                                                                 on new { x.JobUniqueID, x.RouteUniqueID } equals new { y.JobUniqueID, y.RouteUniqueID }
                                                                 join c in gdb.ControlPoint
                                                                 on y.ControlPointUniqueID equals c.UniqueID
                                                                 where x.UniqueID == jobRouteUniqueID && y.ControlPointUniqueID == controlPointUniqueID && jobList.Contains(x.UniqueID)
                                                                 select new
                                                                 {
                                                                     j.OrganizationUniqueID,
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
                                                        var unRFIDReason = gdb.UnRFIDReason.FirstOrDefault(x => x.UniqueID == unRFIDReasonUniqueID);

                                                        var timeSpanAbnormalReasonUniqueID = dt.Rows[i]["TimeSpanAbnormalReasonUniqueID"].ToString();
                                                        var timeSpanAbnormalReason = gdb.TimeSpanAbnormalReason.FirstOrDefault(x => x.UniqueID == timeSpanAbnormalReasonUniqueID);

                                                        var userID = dt.Rows[i]["UserID"].ToString();

                                                        var user = db.User.FirstOrDefault(x => x.ID == userID);

                                                        gdb.ArriveRecord.Add(new ArriveRecord()
                                                        {
                                                            UniqueID = uniqueID,
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
                                                            MinTimeSpan = dt.Rows[i]["MinTimeSpan"] != null && !string.IsNullOrEmpty(dt.Rows[i]["MinTimeSpan"].ToString()) ? double.Parse(dt.Rows[i]["MinTimeSpan"].ToString()) : default(double?),
                                                            TotalTimeSpan = dt.Rows[i]["TotalTimeSpan"] != null && !string.IsNullOrEmpty(dt.Rows[i]["TotalTimeSpan"].ToString()) ? double.Parse(dt.Rows[i]["TotalTimeSpan"].ToString()) : default(double?),
                                                            TimeSpanAbnormalReasonUniqueID = timeSpanAbnormalReasonUniqueID,
                                                            TimeSpanAbnormalReasonID = timeSpanAbnormalReason != null ? timeSpanAbnormalReason.ID : (timeSpanAbnormalReasonUniqueID == "OTHER" ? "OTHER" : ""),
                                                            TimeSpanAbnormalReasonDescription = timeSpanAbnormalReason != null ? timeSpanAbnormalReason.Description : (timeSpanAbnormalReasonUniqueID == "OTHER" ? Resources.Resource.Other : ""),
                                                            TimeSpanAbnormalReasonRemark = dt.Rows[i]["TimeSpanAbnormalReasonRemark"].ToString()
                                                        });

                                                        TransArriveRecordPhoto(gdb, conn, uniqueID, extractPath);
                                                    }
                                                }
                                            }

                                            gdb.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion

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
                                    using (GDbEntities db = new GDbEntities())
                                    {
                                        for (int i = 0; i < dt.Rows.Count; i++)
                                        {
                                            var uniqueID = dt.Rows[i]["UniqueID"].ToString();

                                            if (!db.CheckResult.Any(x => x.UniqueID == uniqueID))
                                            {
                                                var jobRouteUniqueID = dt.Rows[i]["JobUniqueID"].ToString();
                                                var controlPointUniqueID = dt.Rows[i]["ControlPointUniqueID"].ToString();
                                                var checkItemUniqueID = dt.Rows[i]["CheckItemUniqueID"].ToString();

                                                var query = (from x in db.JobControlPointCheckItem
                                                             join checkItem in db.View_ControlPointCheckItem
                                                             on new { x.ControlPointUniqueID, x.CheckItemUniqueID } equals new { checkItem.ControlPointUniqueID, checkItem.CheckItemUniqueID }
                                                             join c in db.ControlPoint
                                                             on x.ControlPointUniqueID equals c.UniqueID
                                                             join y in db.JobRoute
                                                             on new { x.JobUniqueID, x.RouteUniqueID } equals new { y.JobUniqueID, y.RouteUniqueID}
                                                             join j in db.Job
                                                             on y.JobUniqueID equals j.UniqueID
                                                             join r in db.Route
                                                             on y.RouteUniqueID equals r.UniqueID
                                                             where y.UniqueID == jobRouteUniqueID && x.ControlPointUniqueID == controlPointUniqueID && x.CheckItemUniqueID == checkItemUniqueID && jobList.Contains(y.UniqueID)
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

                    //#region UnPatrolRecord
                    //using (SQLiteCommand cmd = conn.CreateCommand())
                    //{
                    //    cmd.CommandText = "SELECT * FROM UnPatrolRecord";

                    //    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                    //    {
                    //        using (DataTable dt = new DataTable())
                    //        {
                    //            adapter.Fill(dt);

                    //            if (dt != null && dt.Rows.Count > 0)
                    //            {
                    //                using (DbEntities db = new DbEntities())
                    //                {
                    //                    using (GDbEntities gdb = new GDbEntities())
                    //                    {
                    //                        for (int i = 0; i < dt.Rows.Count; i++)
                    //                        {
                    //                            var jobRouteUniqueID = dt.Rows[i]["JobUniqueID"].ToString();

                    //                            var unPatrolReasonUniqueID = dt.Rows[i]["UnPatrolReasonUniqueID"].ToString();
                    //                            var unPatrolReason = gdb.UnPatrolReason.FirstOrDefault(x => x.UniqueID == unPatrolReasonUniqueID);

                    //                            var query = gdb.JobRoute.First(x => x.UniqueID == jobRouteUniqueID);

                    //                            gdb.UnPatrolRecord.Add(new UnPatrolRecord()
                    //                            {
                    //                                JobUniqueID = query.JobUniqueID,
                    //                                RouteUniqueID = query.RouteUniqueID,
                    //                                BeginDate = "",
                    //                                EndDate = "",
                    //                                UnPatrolReasonUniqueID = unPatrolReasonUniqueID,

                    //                                LastModifyTime = DateTime.Now
                    //                            });
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //#endregion

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

        private void TransArriveRecordPhoto(GDbEntities DB, SQLiteConnection Conn, string ArriveRecordUniqueID, string ExtractPath)
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

                                    System.IO.File.Copy(photo, Path.Combine(Config.GuardPatrolPhotoFolderPath, ArriveRecordUniqueID + "_" + (i + 1).ToString() + "." + extension), true);

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

        private void TransCheckResultPhoto(GDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID, string ExtractPath)
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

                                    System.IO.File.Copy(photo, Path.Combine(Config.GuardPatrolPhotoFolderPath, CheckResultUniqueID + "_" + (j + 1).ToString() + "." + extension), true);

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

        private void TransCheckResultAbnormalReason(GDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID)
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

        private void TransCheckResultHandlingMethod(GDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID)
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
