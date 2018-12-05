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
using System.Net.Mail;
using System.Text;
using DbEntity.ASE;

namespace SQLite2DB.ASE.QA
{
    public class TransHelper : IDisposable
    {
        #region Recover
        //public void Trans()
        //{
        //    try
        //    {
        //        var zips = Directory.GetFiles(Config.QASQLiteProcessingFolderPath);

        //        Logger.Log(zips.Length + " Files To Trans");

        //        foreach (var zip in zips)
        //        {
        //            FileInfo fileInfo = new FileInfo(zip);

        //            var uploadLogUniqueID = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(fileInfo.Extension));

        //            RequestResult result = Trans(uploadLogUniqueID, fileInfo);

        //            //轉檔成功->搬移到Backup資料夾 & Update Upload Log
        //            if (result.IsSuccess)
        //            {
        //                System.IO.File.Copy(zip, Path.Combine(Config.QASQLiteBackupFolderPath, fileInfo.Name), true);

        //                System.IO.File.Delete(zip);

        //                Directory.Delete(Path.Combine(Config.QASQLiteProcessingFolderPath, uploadLogUniqueID), true);

        //                Logger.Log(zip + " Trans Success");
        //            }
        //            //轉檔失敗->搬移到Error資料夾
        //            else
        //            {
        //                System.IO.File.Copy(zip, Path.Combine(Config.QASQLiteErrorFolderPath, fileInfo.Name), true);

        //                System.IO.File.Delete(zip);

        //                Directory.Delete(Path.Combine(Config.QASQLiteProcessingFolderPath, uploadLogUniqueID), true);

        //                Logger.Log(zip + " Trans Failed");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log(new Error(MethodBase.GetCurrentMethod(), ex));
        //    }
        //}

        //private RequestResult Trans(string UploadLogUniqueID, FileInfo ZipFileInfo)
        //{
        //    RequestResult result = new RequestResult();

        //    try
        //    {
        //        var extractPath = Path.Combine(Config.QASQLiteProcessingFolderPath, UploadLogUniqueID);

        //        using (var zip = ZipFile.Read(ZipFileInfo.FullName))
        //        {
        //            foreach (var entry in zip)
        //            {
        //                entry.Extract(extractPath);
        //            }
        //        }

        //        var connString = string.Format("Data Source={0};Version=3;", Path.Combine(extractPath, Define.SQLite_EquipmentMaintenance));

        //        using (SQLiteConnection conn = new SQLiteConnection(connString))
        //        {
        //            conn.Open();

        //            var uploadList = new List<string>();

        //            #region UploadDefine
        //            using (SQLiteCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = "SELECT * FROM UploadDefine";

        //                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
        //                {
        //                    using (DataTable dt = new DataTable())
        //                    {
        //                        adapter.Fill(dt);

        //                        if (dt != null && dt.Rows.Count > 0)
        //                        {
        //                            uploadList = dt.AsEnumerable().Select(x => x["JobUniqueID"].ToString()).ToList();
        //                        }
        //                    }
        //                }
        //            }
        //            #endregion

        //            #region CheckResult
        //            foreach (var formUniqueID in uploadList)
        //            {
        //                using (SQLiteCommand cmd = conn.CreateCommand())
        //                {
        //                    cmd.CommandText = string.Format("SELECT * FROM CheckResult WHERE JobUniqueID = '{0}'", formUniqueID);

        //                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
        //                    {
        //                        using (DataTable dt = new DataTable())
        //                        {
        //                            adapter.Fill(dt);

        //                            if (dt != null && dt.Rows.Count > 0)
        //                            {
        //                                using (ASEDbEntities db = new ASEDbEntities())
        //                                {
        //                                    for (int i = 0; i < dt.Rows.Count; i++)
        //                                    {
        //                                        var checkResultUniqueID = dt.Rows[i]["UniqueID"].ToString();
        //                                        var checkItemUniqueID = dt.Rows[i]["CheckItemUniqueID"].ToString();

        //                                        var tmp = checkItemUniqueID.Split('_');

        //                                        var checkItemSeq = int.Parse(tmp[1]);

        //                                        var item = db.QA_CALIBRATIONFORMDETAIL.FirstOrDefault(x => x.FORMUNIQUEID == formUniqueID && x.SEQ == checkItemSeq);

        //                                        if (item != null)
        //                                        {
        //                                            TransCheckResultPhoto(db, conn, checkResultUniqueID, formUniqueID, checkItemSeq, extractPath);
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }

        //            }
        //            #endregion

        //            conn.Close();
        //        }

        //        result.Success();
        //    }
        //    catch (Exception ex)
        //    {
        //        var err = new Error(MethodBase.GetCurrentMethod(), ex);

        //        Logger.Log(err);

        //        result.ReturnError(err);
        //    }

        //    return result;
        //}

        //private void TransCheckResultPhoto(ASEDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID, string FormUniqueID, int Seq, string ExtractPath)
        //{
        //    using (SQLiteCommand cmd = Conn.CreateCommand())
        //    {
        //        cmd.CommandText = string.Format("SELECT * FROM CheckResultPhoto WHERE CheckResultUniqueID = '{0}'", CheckResultUniqueID);

        //        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
        //        {
        //            using (DataTable dt = new DataTable())
        //            {
        //                adapter.Fill(dt);

        //                if (dt != null && dt.Rows.Count > 0)
        //                {
        //                    Logger.Log(string.Format("{0} Photos", dt.Rows.Count));
        //                    for (int j = 0; j < dt.Rows.Count; j++)
        //                    {
        //                        var photo = Path.Combine(ExtractPath, dt.Rows[j]["FileName"].ToString());

        //                        if (System.IO.File.Exists(photo))
        //                        {
        //                            var extension = new FileInfo(photo).Extension.Substring(1);

        //                            var fileName = string.Format(string.Format("{0}_{1}_{2}.{3}", FormUniqueID, Seq, j + 1, extension));

        //                            Logger.Log(fileName);

        //                            System.IO.File.Copy(photo, Path.Combine(Config.QAFileFolderPath, fileName), true);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        #endregion

        #region 正常
        public void Trans()
        {
            try
            {
                var folders = Directory.GetDirectories(Config.QASQLiteUploadFolderPath);

                //從Upload資料夾搬移到Processing資料夾
                foreach (var folder in folders)
                {
                    System.IO.File.Move(Path.Combine(folder, "QA.Upload.zip"), Path.Combine(Config.QASQLiteProcessingFolderPath, folder.Substring(folder.LastIndexOf("\\") + 1) + ".zip"));

                    Directory.Delete(folder);
                }

                var zips = Directory.GetFiles(Config.QASQLiteProcessingFolderPath);

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
                            var uploadLog = db.QA_UPLOADLOG.FirstOrDefault(x => x.UNIQUEID == uploadLogUniqueID);

                            if (uploadLog != null)
                            {
                                uploadLog.TRANSTIME = DateTime.Now;

                                db.SaveChanges();
                            }
                        }

                        System.IO.File.Copy(zip, Path.Combine(Config.QASQLiteBackupFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.QASQLiteProcessingFolderPath, uploadLogUniqueID), true);

                        Logger.Log(zip + " Trans Success");
                    }
                    //轉檔失敗->搬移到Error資料夾
                    else
                    {
                        System.IO.File.Copy(zip, Path.Combine(Config.QASQLiteErrorFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.QASQLiteProcessingFolderPath, uploadLogUniqueID), true);

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
                var extractPath = Path.Combine(Config.QASQLiteProcessingFolderPath, UploadLogUniqueID);

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

                    #region CheckResult
                    foreach (var formUniqueID in uploadList)
                    {
                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("SELECT * FROM CheckResult WHERE JobUniqueID = '{0}'", formUniqueID);

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
                                                var checkResultUniqueID = dt.Rows[i]["UniqueID"].ToString();
                                                var checkItemUniqueID = dt.Rows[i]["CheckItemUniqueID"].ToString();

                                                var tmp = checkItemUniqueID.Split('_');

                                                var checkItemSeq = int.Parse(tmp[1]);

                                                var item = db.QA_CALIBRATIONFORMDETAIL.FirstOrDefault(x => x.FORMUNIQUEID == formUniqueID && x.SEQ == checkItemSeq);

                                                if (item != null)
                                                {
                                                    item.CALDATE = DateTimeHelper.DateString2DateTime(dt.Rows[i]["CheckDate"].ToString());

                                                    var value = dt.Rows[i]["Value"].ToString();

                                                    if (!string.IsNullOrEmpty(value))
                                                    {
                                                        decimal val;

                                                        if (decimal.TryParse(value, out val))
                                                        {
                                                            item.READINGVALUE = val;
                                                        }
                                                    }

                                                    var standard = dt.Rows[i]["Standard"].ToString();

                                                    if (!string.IsNullOrEmpty(standard))
                                                    {
                                                        decimal val;

                                                        if (decimal.TryParse(standard, out val))
                                                        {
                                                            item.STANDARD = val;
                                                        }
                                                    }

                                                    TransCheckResultPhoto(db, conn, checkResultUniqueID, formUniqueID, checkItemSeq, extractPath);
                                                }
                                            }

                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }

                        using (SQLiteCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = string.Format("SELECT * FROM STDUSE WHERE CFormUniqueID = '{0}'", formUniqueID);

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
                                                var calno = dt.Rows[i]["CALNO"].ToString();

                                                var equipment = db.QA_EQUIPMENT.FirstOrDefault(x => x.CALNO == calno);

                                                if (equipment != null)
                                                {
                                                    var query = db.QA_CALIBRATIONFORMSTDUSE.FirstOrDefault(x => x.FORMUNIQUEID == formUniqueID && x.EQUIPMENTUNIQUEID == equipment.UNIQUEID);

                                                    if (query == null)
                                                    {
                                                        db.QA_CALIBRATIONFORMSTDUSE.Add(new QA_CALIBRATIONFORMSTDUSE()
                                                        {
                                                            FORMUNIQUEID = formUniqueID,
                                                            EQUIPMENTUNIQUEID = equipment.UNIQUEID
                                                        });
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

                    #region EquipmentPhoto
                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM EquipmentPhoto";

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
                                            var calno = dt.Rows[i]["CALNO"].ToString();

                                            var equipment = db.QA_EQUIPMENT.FirstOrDefault(x => x.CALNO == calno);

                                            if (equipment != null)
                                            {
                                                var photo = Path.Combine(extractPath, dt.Rows[i]["FileName"].ToString());

                                                if (System.IO.File.Exists(photo))
                                                {
                                                    var extension = new FileInfo(photo).Extension.Substring(1);

                                                    var photoName = string.Format("{0}.{1}", equipment.UNIQUEID, extension);

                                                    System.IO.File.Copy(photo, Path.Combine(Config.QAFileFolderPath, photoName), true);

                                                    equipment.PHOTOEXTENSION = extension;
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

        private void TransCheckResultPhoto(ASEDbEntities DB, SQLiteConnection Conn, string CheckResultUniqueID, string FormUniqueID, int Seq, string ExtractPath)
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

                                    System.IO.File.Copy(photo, Path.Combine(Config.QAFileFolderPath, string.Format("{0}_{1}_{2}.{3}", FormUniqueID, Seq, j + 1, extension)), true);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

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
