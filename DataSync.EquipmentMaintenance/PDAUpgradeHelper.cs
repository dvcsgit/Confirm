using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.IO;
using Utility;
using Utility.Models;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using System.Reflection;

namespace DataSync.EquipmentMaintenance
{
    public class PDAUpgradeHelper : IDisposable
    {
        private string Guid = string.Empty;

        private string FilePath
        {
            get
            {
                return Path.Combine(Config.TempFolder, this.Guid);
            }
        }

        private string FileName { get; set; }

        private string UpgradeFile
        {
            get
            {
                return Path.Combine(this.FilePath, FileName);
            }
        }

        private string ZipFile
        {
            get
            {
                return Path.Combine(FilePath, "FEM.Upgrade.zip");
            }
        }

        public PDAUpgradeHelper()
        {
            this.Guid = System.Guid.NewGuid().ToString();
        }

        public RequestResult GenerateUpgradeFileZip()
        {
            RequestResult result = new RequestResult();

            try
            {
                using (EDbEntities db = new EDbEntities())
                {
                    var ver = db.Version.Where(x => x.AppName == "FEM" && x.Device == Define.EnumDevice.WindowsMobile.ToString()).OrderByDescending(x => x.VerCode).FirstOrDefault();

                    FileName = string.Format("{0}.cab", ver.ApkName);
                }

                Directory.CreateDirectory(this.FilePath);

                System.IO.File.Copy(Path.Combine(Config.EquipmentMaintenanceMobileReleaseFolderPath, FileName), UpgradeFile);

                using (ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(ZipFile)))
                {
                    s.SetLevel(9); // 0 - store only to 9 - means best compression
                    byte[] buffer = new byte[4096];

                    ZipEntry entry = new ZipEntry(Path.GetFileName(UpgradeFile));
                    entry.DateTime = DateTime.Now;
                    s.PutNextEntry(entry);
                    using (FileStream fs = System.IO.File.OpenRead(UpgradeFile))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = fs.Read(buffer, 0, buffer.Length);
                            s.Write(buffer, 0, sourceBytes);
                        } while (sourceBytes > 0);
                    }

                    s.Finish();
                    s.Close();
                }

                result.ReturnData(new FileStream(ZipFile, FileMode.Open));
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodInfo.GetCurrentMethod(), ex);

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

        ~PDAUpgradeHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
