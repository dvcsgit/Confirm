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
using DbEntity.MSSQL.TruckPatrol;
using DataAccess;
using DataAccess.TruckPatrol;

namespace SQLite2DB.TruckPatrol
{
    public class TransHelper : IDisposable
    {
        public void Trans()
        {
            try
            {
                var folders = Directory.GetDirectories(Config.TruckPatrolSQLiteUploadFolderPath);

                foreach (var folder in folders)
                {
                    System.IO.File.Move(Path.Combine(folder, "TruckPatrol.Upload.zip"), Path.Combine(Config.TruckPatrolSQLiteProcessingFolderPath, folder.Substring(folder.LastIndexOf("\\") + 1) + ".zip"));

                    Directory.Delete(folder);
                }

                var zips = Directory.GetFiles(Config.TruckPatrolSQLiteProcessingFolderPath);

                Logger.Log(zips.Length + " Files To Trans");

                foreach (var zip in zips)
                {
                    FileInfo fileInfo = new FileInfo(zip);

                    var uploadLogUniqueID = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(fileInfo.Extension));

                    RequestResult result = Trans(uploadLogUniqueID, fileInfo);

                    if (result.IsSuccess)
                    {
                        using (TDbEntities db = new TDbEntities())
                        {

                            var uploadLog = db.UploadLog.Where(x => x.UniqueID == uploadLogUniqueID).FirstOrDefault();
                            if(uploadLog != null)
                            {
                                uploadLog.TransTime = DateTime.Now;
                                db.SaveChanges();
                            }
                            else
                            {
                                Logger.Log("warn: uploadLog not exist : " + uploadLogUniqueID);
                            }

                            
var log = db.UploadLog.FirstOrDefault(x => x.UniqueID == uploadLogUniqueID);

                            if (log != null)
                            {
                                log.TransTime = DateTime.Now;

                                db.SaveChanges();
                            }
                        }

                        System.IO.File.Copy(zip, Path.Combine(Config.TruckPatrolSQLiteBackupFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.TruckPatrolSQLiteProcessingFolderPath, uploadLogUniqueID), true);

                        Logger.Log(zip + " Trans Success");
                    }
                    else
                    {
                        System.IO.File.Copy(zip, Path.Combine(Config.TruckPatrolSQLiteErrorFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.TruckPatrolSQLiteProcessingFolderPath, uploadLogUniqueID), true);

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
                var extractPath = Path.Combine(Config.TruckPatrolSQLiteProcessingFolderPath, UploadLogUniqueID);

                using (var zip = ZipFile.Read(ZipFileInfo.FullName))
                {
                    foreach (var entry in zip)
                    {
                        entry.Extract(extractPath, ExtractExistingFileAction.OverwriteSilently);
                    }
                }

                var connString = string.Format("Data Source={0};Version=3;", Path.Combine(extractPath, Define.SQLite_TruckPatrol));

                using (SQLiteConnection conn = new SQLiteConnection(connString))
                {
                    conn.Open();

                    var truckBindingList = new List<string>();

                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT UniqueID FROM TruckBinding";

                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                adapter.Fill(dt);

                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    truckBindingList = dt.AsEnumerable().Select(x => x["UniqueID"].ToString()).ToList();
                                }
                            }
                        }
                    }

                    //#region UploadDefine
                    //using (SQLiteCommand cmd = conn.CreateCommand())
                    //{
                    //    cmd.CommandText = "SELECT * FROM UploadDefine";

                    //    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                    //    {
                    //        using (DataTable dt = new DataTable())
                    //        {
                    //            adapter.Fill(dt);

                    //            if (dt != null && dt.Rows.Count > 0)
                    //            {
                    //                truckBindingList = dt.AsEnumerable().Select(x => x["TruckBindingUniqueID"].ToString()).ToList();
                    //            }
                    //        }
                    //    }
                    //}
                    //#endregion

                    foreach (var truckBinding in truckBindingList)
                    {
                        var firstTruckUniqueID = string.Empty;
                        var secondTruckUniqueID = string.Empty;

                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("SELECT * FROM TruckBinding WHERE UniqueID = '{0}'", truckBinding);

                            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                            {
                                using (DataTable dt = new DataTable())
                                {
                                    adapter.Fill(dt);

                                    if (dt != null && dt.Rows.Count > 0)
                                    {
                                        firstTruckUniqueID = dt.Rows[0]["FirstTruckUniqueID"].ToString();
                                        secondTruckUniqueID = dt.Rows[0]["SecondTruckUniqueID"].ToString();
                                    }
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(firstTruckUniqueID))
                        {
                            TransArriveRecord(conn, truckBinding, firstTruckUniqueID, extractPath);
                        }

                        if (!string.IsNullOrEmpty(secondTruckUniqueID))
                        {
                            TransArriveRecord(conn, truckBinding, secondTruckUniqueID, extractPath);
                        }

                        TruckBindingResultHelper.Insert(truckBinding);
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

        private void TransArriveRecord(SQLiteConnection Conn, string TruckBindingUniqueID, string TruckUniqueID, string ExtractPath)
        {
            using (SQLiteCommand cmd = Conn.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM ArriveRecord WHERE TruckUniqueID = '{0}'", TruckUniqueID);

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                {
                    using (DataTable dt = new DataTable())
                    {
                        adapter.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            using (DbEntities db = new DbEntities())
                            {
                                using (TDbEntities tdb = new TDbEntities())
                                {
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        var uniqueID = dt.Rows[i]["UniqueID"].ToString();

                                        if (!tdb.ArriveRecord.Any(x => x.UniqueID == uniqueID))
                                        {
                                            var controlPointUniqueID = dt.Rows[i]["ControlPointUniqueID"].ToString();

                                            var query = (from c in tdb.ControlPoint
                                                         join t in tdb.Truck
                                                         on c.TruckUniqueID equals t.UniqueID
                                                         where c.UniqueID == controlPointUniqueID
                                                         select new
                                                         {
                                                             t.OrganizationUniqueID,
                                                             TruckUniqueID = t.UniqueID,
                                                             t.TruckNo,
                                                             t.BindingType,
                                                             ControlPointUniqueID = c.UniqueID,
                                                             ControlPointID = c.ID,
                                                             ControlPointDescription = c.Description
                                                         }).FirstOrDefault();

                                            if (query != null)
                                            {
                                                var unRFIDReasonUniqueID = dt.Rows[i]["UnRFIDReasonUniqueID"].ToString();
                                                var unRFIDReason = tdb.UnRFIDReason.FirstOrDefault(x => x.UniqueID == unRFIDReasonUniqueID);

                                                var userID = dt.Rows[i]["UserID"].ToString();

                                                var user = db.User.FirstOrDefault(x => x.ID == userID);

                                                tdb.ArriveRecord.Add(new ArriveRecord()
                                                {
                                                    UniqueID = uniqueID,
                                                    OrganizationUniqueID = query.OrganizationUniqueID,
                                                    TruckBindingUniqueID = TruckBindingUniqueID,
                                                    TruckBindingType = query.BindingType,
                                                    TruckUniqueID = query.TruckUniqueID,
                                                    TruckNo = query.TruckNo,
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
                                                    UnRFIDReasonRemark = dt.Rows[i]["UnRFIDReasonRemark"].ToString()
                                                });

                                                TransArriveRecordPhoto(tdb, Conn, uniqueID, ExtractPath);
                                                TransCheckResult(tdb, Conn, uniqueID, ExtractPath);
                                            }
                                        }
                                    }

                                    tdb.SaveChanges();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TransArriveRecordPhoto(TDbEntities DB, SQLiteConnection Conn, string ArriveRecordUniqueID, string ExtractPath)
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

                                    System.IO.File.Copy(photo, Path.Combine(Config.TruckPatrolPhotoFolderPath, ArriveRecordUniqueID + "_" + (i + 1).ToString() + "." + extension), true);

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

        private void TransCheckResult(TDbEntities DB, SQLiteConnection Conn, string ArriveRecordUniqueID, string ExtractPath)
        {
            using (SQLiteCommand cmd = Conn.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM CheckResult WHERE ArriveRecordUniqueID = '{0}'", ArriveRecordUniqueID);

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                {
                    using (DataTable dt = new DataTable())
                    {
                        adapter.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            using (TDbEntities db = new TDbEntities())
                            {
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    var uniqueID = dt.Rows[i]["UniqueID"].ToString();

                                    if (!db.CheckResult.Any(x => x.UniqueID == uniqueID))
                                    {
                                        var controlPointUniqueID = dt.Rows[i]["ControlPointUniqueID"].ToString();
                                        var checkItemUniqueID = dt.Rows[i]["CheckItemUniqueID"].ToString();

                                        var query = (from x in db.View_ControlPointCheckItem
                                                     where x.ControlPointUniqueID == controlPointUniqueID && x.CheckItemUniqueID == checkItemUniqueID
                                                     select new
                                                     {
                                                         x.ControlPointUniqueID,
                                                         CheckItemUniqueID = x.CheckItemUniqueID,
                                                         CheckItemID = x.ID,
                                                         CheckItemDescription = x.Description,
                                                         x.LowerLimit,
                                                         x.LowerAlertLimit,
                                                         x.UpperAlertLimit,
                                                         x.UpperLimit,
                                                         x.AccumulationBase,
                                                         x.Unit,
                                                         x.Remark
                                                     }).FirstOrDefault();

                                        if (query != null)
                                        {
                                            var checkResult = new CheckResult()
                                            {
                                                UniqueID = uniqueID,
                                                ArriveRecordUniqueID = dt.Rows[i]["ArriveRecordUniqueID"].ToString(),
                                                CheckItemUniqueID = query.CheckItemUniqueID,
                                                CheckItemID = query.CheckItemID,
                                                CheckItemDescription = query.CheckItemDescription,
                                                LowerLimit = query.LowerLimit,
                                                LowerAlertLimit = query.LowerAlertLimit,
                                                UpperAlertLimit = query.UpperAlertLimit,
                                                UpperLimit = query.UpperLimit,
                                                Unit = query.Unit,
                                                Remark = query.Remark,
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
                                                    var prevCheckResult = (from c in db.CheckResult
                                                                           join a in db.ArriveRecord
                                                                           on c.ArriveRecordUniqueID equals a.UniqueID
                                                                           where a.ControlPointUniqueID == query.ControlPointUniqueID && c.CheckItemUniqueID == query.CheckItemUniqueID && c.Value.HasValue
                                                                           select c).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();
                                                        
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

                                            db.CheckResult.Add(checkResult);

                                            TransCheckResultPhoto(db, Conn, uniqueID, ExtractPath);

                                            TransCheckResultAbnormalReason(db, Conn, uniqueID);

                                            TransCheckResultHandlingMethod(db, Conn, uniqueID);
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

        private void TransCheckResultPhoto(TDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID, string ExtractPath)
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

                                    System.IO.File.Copy(photo, Path.Combine(Config.TruckPatrolPhotoFolderPath, CheckResultUniqueID + "_" + (j + 1).ToString() + "." + extension), true);

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

        private void TransCheckResultAbnormalReason(TDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID)
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

        private void TransCheckResultHandlingMethod(TDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID)
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
