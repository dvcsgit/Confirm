using DataAccess;
using DbEntity.MSSQL.TankPatrol;
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

namespace SQLite2DB.TankPatrol
{
    //public class TransHelper : IDisposable
    //{
    //    public void Trans()
    //    {
    //        try
    //        {
    //            var folders = Directory.GetDirectories(Config.TankPatrolSQLiteUploadFolderPath);

    //            //從Upload資料夾搬移到Processing資料夾
    //            foreach (var folder in folders)
    //            {
    //                System.IO.File.Move(Path.Combine(folder, "TankPatrol.Upload.zip"), Path.Combine(Config.TankPatrolSQLiteProcessingFolderPath, folder.Substring(folder.LastIndexOf("\\") + 1) + ".zip"));

    //                Directory.Delete(folder);
    //            }

    //            var zips = Directory.GetFiles(Config.TankPatrolSQLiteProcessingFolderPath);

    //            Logger.Log(zips.Length + " Files To Trans");

    //            foreach (var zip in zips)
    //            {
    //                FileInfo fileInfo = new FileInfo(zip);

    //                var uploadLogUniqueID = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(fileInfo.Extension));

    //                RequestResult result = Trans(uploadLogUniqueID, fileInfo);

    //                //轉檔成功->搬移到Backup資料夾 & Update Upload Log
    //                if (result.IsSuccess)
    //                {
    //                    System.IO.File.Copy(zip, Path.Combine(Config.TankPatrolSQLiteBackupFolderPath, fileInfo.Name), true);

    //                    System.IO.File.Delete(zip);

    //                    Directory.Delete(Path.Combine(Config.TankPatrolSQLiteProcessingFolderPath, uploadLogUniqueID), true);

    //                    Logger.Log(zip + " Trans Success");
    //                }
    //                //轉檔失敗->搬移到Error資料夾
    //                else
    //                {
    //                    System.IO.File.Copy(zip, Path.Combine(Config.TankPatrolSQLiteErrorFolderPath, fileInfo.Name), true);

    //                    System.IO.File.Delete(zip);

    //                    Directory.Delete(Path.Combine(Config.TankPatrolSQLiteProcessingFolderPath, uploadLogUniqueID), true);

    //                    Logger.Log(zip + " Trans Failed");
    //                }

    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            Logger.Log(new Error(MethodBase.GetCurrentMethod(), ex));
    //        }
    //    }

    //    private RequestResult Trans(string UploadLogUniqueID, FileInfo ZipFileInfo)
    //    {
    //        RequestResult result = new RequestResult();

    //        try
    //        {
    //            var extractPath = Path.Combine(Config.TankPatrolSQLiteProcessingFolderPath, UploadLogUniqueID);

    //            using (var zip = ZipFile.Read(ZipFileInfo.FullName))
    //            {
    //                foreach (var entry in zip)
    //                {
    //                    entry.Extract(extractPath);
    //                }
    //            }

    //            var connString = string.Format("Data Source={0};Version=3;", Path.Combine(extractPath, Define.SQLite_TankPatrol));

    //            using (SQLiteConnection conn = new SQLiteConnection(connString))
    //            {
    //                conn.Open();

    //                using (SQLiteCommand cmd = conn.CreateCommand())
    //                {
    //                    cmd.CommandText = "SELECT * FROM CheckResult";

    //                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
    //                    {
    //                        using (DataTable dt = new DataTable())
    //                        {
    //                            adapter.Fill(dt);

    //                            if (dt != null && dt.Rows.Count > 0)
    //                            {
    //                                using (TankDbEntities db = new TankDbEntities())
    //                                {
    //                                    for (int rowIndex = 0; rowIndex < dt.Rows.Count; rowIndex++)
    //                                    {
    //                                        var checkResultUniqueID = dt.Rows[rowIndex]["UniqueID"].ToString();

    //                                        var checkResult = db.CheckResult.FirstOrDefault(x => x.UniqueID == checkResultUniqueID);

    //                                        if (checkResult != null && string.IsNullOrEmpty(checkResult.Result))
    //                                        {
    //                                            var value = dt.Rows[rowIndex]["Value"].ToString();

    //                                            if (!string.IsNullOrEmpty(value))
    //                                            {
    //                                                double val = double.Parse(value);
    //                                                double netVal = val;

    //                                                checkResult.Value = val;



    //                                                checkResult.Result = netVal.ToString("F2");
    //                                            }
    //                                        }
    //                                    }

    //                                    db.SaveChanges();
    //                                }

    //                            }
    //                        }
    //                    }
    //                }

    //                conn.Close();
    //            }

    //            result.Success();
    //        }
    //        catch (Exception ex)
    //        {
    //            var err = new Error(MethodBase.GetCurrentMethod(), ex);

    //            Logger.Log(err);

    //            result.ReturnError(err);
    //        }

    //        return result;
    //    }

    //    #region IDisposable

    //    private bool _disposed = false;

    //    public void Dispose()
    //    {
    //        Dispose(true);

    //        GC.SuppressFinalize(this);
    //    }

    //    protected virtual void Dispose(bool disposing)
    //    {
    //        if (!_disposed)
    //        {
    //            if (disposing)
    //            {

    //            }
    //        }

    //        _disposed = true;
    //    }

    //    ~TransHelper()
    //    {
    //        Dispose(false);
    //    }

    //    #endregion
    //}

    public class TransHelper : IDisposable
    {
        public void Trans()
        {
            try
            {
                var folders = Directory.GetDirectories(Config.TankPatrolSQLiteUploadFolderPath);

                //從Upload資料夾搬移到Processing資料夾
                foreach (var folder in folders)
                {
                    System.IO.File.Move(Path.Combine(folder, "TankPatrol.Upload.zip"), Path.Combine(Config.TankPatrolSQLiteProcessingFolderPath, folder.Substring(folder.LastIndexOf("\\") + 1) + ".zip"));

                    Directory.Delete(folder);
                }

                var zips = Directory.GetFiles(Config.TankPatrolSQLiteProcessingFolderPath);

                Logger.Log(zips.Length + " Files To Trans");

                foreach (var zip in zips)
                {
                    FileInfo fileInfo = new FileInfo(zip);

                    var uploadLogUniqueID = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(fileInfo.Extension));

                    RequestResult result = Trans(uploadLogUniqueID, fileInfo);

                    //轉檔成功->搬移到Backup資料夾 & Update Upload Log
                    if (result.IsSuccess)
                    {
                        using (TankDbEntities db = new TankDbEntities())
                        {
                            var uploadLog = db.UploadLog.FirstOrDefault(x => x.UniqueID == uploadLogUniqueID);

                            if (uploadLog != null)
                            {
                                uploadLog.TransTime = DateTime.Now;

                                db.SaveChanges();
                            }
                        }

                        System.IO.File.Copy(zip, Path.Combine(Config.TankPatrolSQLiteBackupFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.TankPatrolSQLiteProcessingFolderPath, uploadLogUniqueID), true);

                        Logger.Log(zip + " Trans Success");
                    }
                    //轉檔失敗->搬移到Error資料夾
                    else
                    {
                        System.IO.File.Copy(zip, Path.Combine(Config.TankPatrolSQLiteErrorFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.TankPatrolSQLiteProcessingFolderPath, uploadLogUniqueID), true);

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
                var extractPath = Path.Combine(Config.TankPatrolSQLiteProcessingFolderPath, UploadLogUniqueID);

                using (var zip = ZipFile.Read(ZipFileInfo.FullName))
                {
                    foreach (var entry in zip)
                    {
                        entry.Extract(extractPath);
                    }
                }

                var connString = string.Format("Data Source={0};Version=3;", Path.Combine(extractPath, Define.SQLite_TankPatrol));

                using (SQLiteConnection conn = new SQLiteConnection(connString))
                {
                    conn.Open();

                    #region ArriveRecord

                    using (SQLiteCommand cmdArriveRecord = conn.CreateCommand())
                    {
                        cmdArriveRecord.CommandText = "SELECT * FROM ArriveRecord";

                        using (SQLiteDataAdapter adapterArriveRecord = new SQLiteDataAdapter(cmdArriveRecord))
                        {
                            using (DataTable dtArriveRecord = new DataTable())
                            {
                                adapterArriveRecord.Fill(dtArriveRecord);

                                if (dtArriveRecord != null && dtArriveRecord.Rows.Count > 0)
                                {
                                    using (TankDbEntities db = new TankDbEntities())
                                    {
                                        for (int arriveRecordRowIndex = 0; arriveRecordRowIndex < dtArriveRecord.Rows.Count; arriveRecordRowIndex++)
                                        {
                                            var arriveRecordUniqueID = dtArriveRecord.Rows[arriveRecordRowIndex]["UniqueID"].ToString();

                                            if (!db.ArriveRecord.Any(x => x.UniqueID == arriveRecordUniqueID))
                                            {
                                                var portUniqueID = dtArriveRecord.Rows[arriveRecordRowIndex]["PortUniqueID"].ToString();

                                                var port = (from p in db.Port
                                                            join i in db.Island
                                                            on p.IslandUniqueID equals i.UniqueID
                                                            join s in db.Station
                                                            on i.StationUniqueID equals s.UniqueID
                                                            where p.UniqueID == portUniqueID
                                                            select new
                                                            {
                                                                s.OrganizationUniqueID,
                                                                StationUniqueID = s.UniqueID,
                                                                StationID = s.ID,
                                                                StationDescription = s.Description,
                                                                IslandUniqueID = i.UniqueID,
                                                                IslandID = i.ID,
                                                                IslandDescription = i.Description,
                                                                PortUniqueID = p.UniqueID,
                                                                PortID = p.ID,
                                                                PortDescription = p.Description
                                                            }).FirstOrDefault();

                                                if (port != null)
                                                {
                                                    var unRFIDReasonUniqueID = dtArriveRecord.Rows[arriveRecordRowIndex]["UnRFIDReasonUniqueID"].ToString();
                                                    var unRFIDReason = db.UnRFIDReason.FirstOrDefault(x => x.UniqueID == unRFIDReasonUniqueID);

                                                    var userID = dtArriveRecord.Rows[arriveRecordRowIndex]["UserID"].ToString();

                                                    var user = UserDataAccessor.GetUser(userID);
                                                    var organization = OrganizationDataAccessor.GetOrganization(port.OrganizationUniqueID);

                                                    var arriveRecord = new ArriveRecord()
                                                    {
                                                        UniqueID = arriveRecordUniqueID,
                                                        OrganizationUniqueID = port.OrganizationUniqueID,
                                                        OrganizationDescription = organization.Description,
                                                        StationUniqueID = port.StationUniqueID,
                                                        StationID = port.StationID,
                                                        StationDescription = port.StationDescription,
                                                        IslandUniqueID = port.IslandUniqueID,
                                                        IslandID = port.IslandID,
                                                        IslandDescription = port.IslandDescription,
                                                        PortUniqueID = port.PortUniqueID,
                                                        PortID = port.PortID,
                                                        PortDescription = port.PortDescription,
                                                        CheckType = dtArriveRecord.Rows[arriveRecordRowIndex]["CheckType"].ToString(),
                                                        TankNo = dtArriveRecord.Rows[arriveRecordRowIndex]["TankNo"].ToString(),
                                                        Driver = dtArriveRecord.Rows[arriveRecordRowIndex]["Driver"].ToString(),
                                                        Owner = dtArriveRecord.Rows[arriveRecordRowIndex]["Owner"].ToString(),
                                                        LastTimeMaterial = dtArriveRecord.Rows[arriveRecordRowIndex]["LastTimeMaterial"].ToString(),
                                                        ThisTimeMaterial = dtArriveRecord.Rows[arriveRecordRowIndex]["ThisTimeMaterial"].ToString(),
                                                        ArriveDate = dtArriveRecord.Rows[arriveRecordRowIndex]["ArriveDate"].ToString(),
                                                        ArriveTime = dtArriveRecord.Rows[arriveRecordRowIndex]["ArriveTime"].ToString(),
                                                        UserID = userID,
                                                        UserName = user != null ? user.Name : "",
                                                        UnRFIDReasonUniqueID = unRFIDReasonUniqueID,
                                                        UnRFIDReasonID = unRFIDReason != null ? unRFIDReason.ID : (unRFIDReasonUniqueID == "OTHER" ? "OTHER" : ""),
                                                        UnRFIDReasonDescription = unRFIDReason != null ? unRFIDReason.Description : (unRFIDReasonUniqueID == "OTHER" ? Resources.Resource.Other : ""),
                                                        UnRFIDReasonRemark = dtArriveRecord.Rows[arriveRecordRowIndex]["UnRFIDReasonRemark"].ToString()
                                                    };

                                                    TransArriveRecordPhoto(db, conn, arriveRecord, extractPath);

                                                    db.ArriveRecord.Add(arriveRecord);

                                                    #region CheckResult
                                                    using (SQLiteCommand cmdCheckResult = conn.CreateCommand())
                                                    {
                                                        cmdCheckResult.CommandText = string.Format("SELECT * FROM CheckResult WHERE ArriveRecordUniqueID = '{0}'", arriveRecord.UniqueID);

                                                        using (SQLiteDataAdapter adapterCheckResult = new SQLiteDataAdapter(cmdCheckResult))
                                                        {
                                                            using (DataTable dtCheckResult = new DataTable())
                                                            {
                                                                adapterCheckResult.Fill(dtCheckResult);

                                                                if (dtCheckResult != null && dtCheckResult.Rows.Count > 0)
                                                                {
                                                                    for (int checkResultRowIndex = 0; checkResultRowIndex < dtCheckResult.Rows.Count; checkResultRowIndex++)
                                                                    {
                                                                        var checkResultUniqueID = dtCheckResult.Rows[checkResultRowIndex]["UniqueID"].ToString();

                                                                        if (!db.CheckResult.Any(x => x.UniqueID == checkResultUniqueID))
                                                                        {
                                                                            var checkItemUniqueID = dtCheckResult.Rows[checkResultRowIndex]["CheckItemUniqueID"].ToString();
                                                                            var procedure = dtCheckResult.Rows[checkResultRowIndex]["Procedure"].ToString();

                                                                            var checkItem = (from x in db.PortCheckItem
                                                                                             join c in db.CheckItem
                                                                                             on x.CheckItemUniqueID equals c.UniqueID
                                                                                             where x.PortUniqueID == arriveRecord.PortUniqueID && x.CheckItemUniqueID == checkItemUniqueID && x.CheckType == arriveRecord.CheckType && x.Procedure == procedure
                                                                                             select new
                                                                                             {
                                                                                                 CheckItemUniqueID = c.UniqueID,
                                                                                                 CheckItemID = c.ID,
                                                                                                 CheckItemDescription = c.Description,
                                                                                                 LowerLimit = x.IsInherit ? c.LowerLimit : x.LowerLimit,
                                                                                                 LowerAlertLimit = x.IsInherit ? c.LowerAlertLimit : x.LowerAlertLimit,
                                                                                                 UpperAlertLimit = x.IsInherit ? c.UpperAlertLimit : x.UpperAlertLimit,
                                                                                                 UpperLimit = x.IsInherit ? c.UpperLimit : x.UpperLimit,
                                                                                                 AccumulationBase = x.IsInherit ? c.AccumulationBase : x.AccumulationBase,
                                                                                                 Unit = x.IsInherit ? c.Unit : x.Unit,
                                                                                                 c.TextValueType,
                                                                                                 c.IsFeelItem
                                                                                             }).FirstOrDefault();

                                                                            if (checkItem != null)
                                                                            {
                                                                                var checkResult = new CheckResult()
                                                                                {
                                                                                    UniqueID = checkResultUniqueID,
                                                                                    ArriveRecordUniqueID = arriveRecord.UniqueID,
                                                                                    OrganizationUniqueID = port.OrganizationUniqueID,
                                                                                    OrganizationDescription = arriveRecord.OrganizationDescription,
                                                                                    StationUniqueID = arriveRecord.StationUniqueID,
                                                                                    StationID = arriveRecord.StationID,
                                                                                    StationDescription = arriveRecord.StationDescription,
                                                                                    IslandUniqueID = arriveRecord.IslandUniqueID,
                                                                                    IslandID = arriveRecord.IslandID,
                                                                                    IslandDescription = arriveRecord.IslandDescription,
                                                                                    PortUniqueID = arriveRecord.PortUniqueID,
                                                                                    PortID = arriveRecord.PortID,
                                                                                    PortDescription = arriveRecord.PortDescription,
                                                                                    CheckItemUniqueID = checkItem.CheckItemUniqueID,
                                                                                    CheckItemID = checkItem.CheckItemID,
                                                                                    CheckItemDescription = checkItem.CheckItemDescription,
                                                                                    Procedure = procedure,
                                                                                    LowerLimit = checkItem.LowerLimit,
                                                                                    LowerAlertLimit = checkItem.LowerAlertLimit,
                                                                                    UpperAlertLimit = checkItem.UpperAlertLimit,
                                                                                    UpperLimit = checkItem.UpperLimit,
                                                                                    Unit = checkItem.Unit,
                                                                                    CheckDate = dtCheckResult.Rows[checkResultRowIndex]["CheckDate"].ToString(),
                                                                                    CheckTime = dtCheckResult.Rows[checkResultRowIndex]["CheckTime"].ToString(),
                                                                                    FeelOptionUniqueID = dtCheckResult.Rows[checkResultRowIndex]["FeelOptionUniqueID"].ToString(),
                                                                                    IsAbnormal = false,
                                                                                    IsAlert = false
                                                                                };

                                                                                if (!checkItem.IsFeelItem)
                                                                                {
                                                                                    if (checkItem.TextValueType == 1)
                                                                                    {
                                                                                        var value = dtCheckResult.Rows[checkResultRowIndex]["Value"].ToString();

                                                                                        if (!string.IsNullOrEmpty(value))
                                                                                        {
                                                                                            double val = double.Parse(value);
                                                                                            double netVal = val;

                                                                                            checkResult.Value = val;

                                                                                            if (checkItem.AccumulationBase.HasValue)
                                                                                            {
                                                                                                var prevCheckResult = db.CheckResult.Where(x => x.PortUniqueID == arriveRecord.PortUniqueID && x.CheckItemUniqueID == checkItem.CheckItemUniqueID && x.Value.HasValue).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                                                                                                if (prevCheckResult != null)
                                                                                                {
                                                                                                    netVal = val - prevCheckResult.Value.Value;
                                                                                                    checkResult.NetValue = netVal;
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    netVal = val - checkItem.AccumulationBase.Value;
                                                                                                    checkResult.NetValue = netVal;
                                                                                                }
                                                                                            }

                                                                                            checkResult.Result = netVal.ToString("F2");

                                                                                            if (checkItem.UpperLimit.HasValue && netVal > checkItem.UpperLimit.Value)
                                                                                            {
                                                                                                checkResult.IsAbnormal = true;
                                                                                            }

                                                                                            if (checkItem.UpperAlertLimit.HasValue && netVal > checkItem.UpperAlertLimit.Value)
                                                                                            {
                                                                                                checkResult.IsAlert = true;
                                                                                            }

                                                                                            if (checkItem.LowerLimit.HasValue && netVal < checkItem.LowerLimit.Value)
                                                                                            {
                                                                                                checkResult.IsAbnormal = true;
                                                                                            }

                                                                                            if (checkItem.LowerAlertLimit.HasValue && netVal < checkItem.LowerAlertLimit.Value)
                                                                                            {
                                                                                                checkResult.IsAlert = true;
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        var textValue = dtCheckResult.Rows[checkResultRowIndex]["TextValue"].ToString();

                                                                                        checkResult.Result = textValue;
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

                                                                                TransCheckResultPhoto(db, conn, arriveRecordUniqueID, extractPath);

                                                                                TransCheckResultAbnormalReason(db, conn, arriveRecordUniqueID);

                                                                                TransCheckResultHandlingMethod(db, conn, arriveRecordUniqueID);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    #endregion
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

        private void TransArriveRecordPhoto(TankDbEntities DB, SQLiteConnection Conn, ArriveRecord ArriveRecord, string ExtractPath)
        {
            using (SQLiteCommand cmd = Conn.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM ArriveRecordPhoto WHERE ArriveRecordUniqueID = '{0}' AND Type = '1'", ArriveRecord.UniqueID);

                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                {
                    using (DataTable dt = new DataTable())
                    {
                        adapter.Fill(dt);

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            var photo = Path.Combine(ExtractPath, dt.Rows[0]["FileName"].ToString());

                            if (System.IO.File.Exists(photo))
                            {
                                var extension = new FileInfo(photo).Extension.Substring(1);

                                System.IO.File.Copy(photo, Path.Combine(Config.TankPatrolPhotoFolderPath, string.Format("{0}.{1}", ArriveRecord.UniqueID, extension)), true);

                                ArriveRecord.SignExtension = extension;
                            }
                        }
                    }
                }
            }

            using (SQLiteCommand cmd = Conn.CreateCommand())
            {
                cmd.CommandText = string.Format("SELECT * FROM ArriveRecordPhoto WHERE ArriveRecordUniqueID = '{0}' AND Type = '0'", ArriveRecord.UniqueID);

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

                                    System.IO.File.Copy(photo, Path.Combine(Config.TankPatrolPhotoFolderPath, string.Format("{0}_{1}.{2}", ArriveRecord.UniqueID, i + 1, extension)), true);

                                    DB.ArriveRecordPhoto.Add(new ArriveRecordPhoto()
                                    {
                                        ArriveRecordUniqueID = ArriveRecord.UniqueID,
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

        private void TransCheckResultPhoto(TankDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID, string ExtractPath)
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

                                    System.IO.File.Copy(photo, Path.Combine(Config.TankPatrolPhotoFolderPath, CheckResultUniqueID + "_" + (j + 1).ToString() + "." + extension), true);

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

        private void TransCheckResultAbnormalReason(TankDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID)
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

        private void TransCheckResultHandlingMethod(TankDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID)
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
