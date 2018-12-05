using DbEntity.MSSQL.GuardPatrol;
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

namespace TagSQLite2DB.GuardPatrol
{
    public class TransHelper : IDisposable
    {
        public void Trans()
        {
            try
            {
                var folders = Directory.GetDirectories(Config.GuardPatrolTagSQLiteUploadFolderPath);

                foreach (var folder in folders)
                {
                    System.IO.File.Move(Path.Combine(folder, "GuardPatrol.Tag.Upload.zip"), Path.Combine(Config.GuardPatrolTagSQLiteProcessingFolderPath, folder.Substring(folder.LastIndexOf("\\") + 1) + ".zip"));

                    Directory.Delete(folder);
                }

                var zips = Directory.GetFiles(Config.GuardPatrolTagSQLiteProcessingFolderPath);

                Logger.Log(zips.Length + " Files To Trans");

                foreach (var zip in zips)
                {
                    FileInfo fileInfo = new FileInfo(zip);

                    var uniqueID = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(fileInfo.Extension));

                    RequestResult result = Trans(uniqueID, fileInfo);

                    if (result.IsSuccess)
                    {
                        System.IO.File.Copy(zip, Path.Combine(Config.GuardPatrolTagSQLiteBackupFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.GuardPatrolTagSQLiteProcessingFolderPath, uniqueID), true);

                        Logger.Log(zip + " Trans Success");
                    }
                    else
                    {
                        System.IO.File.Copy(zip, Path.Combine(Config.GuardPatrolTagSQLiteErrorFolderPath, fileInfo.Name), true);

                        System.IO.File.Delete(zip);

                        Directory.Delete(Path.Combine(Config.GuardPatrolTagSQLiteProcessingFolderPath, uniqueID), true);

                        Logger.Log(zip + " Trans Failed");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(new Error(MethodBase.GetCurrentMethod(), ex));
            }
        }

        private RequestResult Trans(string UniqueID, FileInfo ZipFileInfo)
        {
            RequestResult result = new RequestResult();

            try
            {
                var extractPath = Path.Combine(Config.GuardPatrolTagSQLiteProcessingFolderPath, UniqueID);

                using (var zip = ZipFile.Read(ZipFileInfo.FullName))
                {
                    foreach (var entry in zip)
                    {
                        entry.Extract(extractPath);
                    }
                }

                var connString = string.Format("Data Source={0};Version=3;", Path.Combine(extractPath, Define.SQLite_GuardPatrolTag));

                using (SQLiteConnection conn = new SQLiteConnection(connString))
                {
                    conn.Open();

                    using (SQLiteCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM ControlPoint";

                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                        {
                            using (DataTable dt = new DataTable())
                            {
                                adapter.Fill(dt);

                                var itemList = dt.AsEnumerable().Select(x => new
                                {
                                    UniqueID = x["UniqueID"].ToString(),
                                    TagID = x["TagID"].ToString()
                                }).ToList();

                                using (GDbEntities db = new GDbEntities())
                                {
                                    foreach (var item in itemList)
                                    {
                                        if (!string.IsNullOrEmpty(item.TagID))
                                        {
                                            var controlPoint = db.ControlPoint.FirstOrDefault(x => x.UniqueID == item.UniqueID);

                                            if (controlPoint != null)
                                            {
                                                controlPoint.TagID = item.TagID;
                                            }
                                        }
                                    }

                                    db.SaveChanges();
                                }
                            }
                        }
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
