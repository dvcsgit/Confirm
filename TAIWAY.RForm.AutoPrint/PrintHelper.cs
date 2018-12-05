using DataAccess.EquipmentMaintenance;
using DbEntity.MSSQL.EquipmentMaintenance;
using Models.EquipmentMaintenance.RepairFormManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utility;
using Utility.Models;

namespace TAIWAY.RForm.AutoPrint
{
    public class PrintHelper : IDisposable
    {
        private string PrintSettingFileName = "PrintSetting.xml";

        private string PrintSettingFile
        {
            get
            {
                string exePath = System.AppDomain.CurrentDomain.BaseDirectory;

                string filePath = Path.Combine(exePath, PrintSettingFileName);

                if (System.IO.File.Exists(filePath))
                {
                    return filePath;
                }
                else
                {
                    filePath = Path.Combine(exePath, "bin", PrintSettingFileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        return filePath;
                    }
                    else
                    {
                        return PrintSettingFileName;
                    }
                }
            }
        }

        private int Interval
        {
            get
            {
                var doc = XDocument.Load(PrintSettingFile);

                return int.Parse(doc.Root.Element("Interval").Value);
            }
        }

        public void Print()
        {
            try
            {
                var execTime = DateTime.Now;

                var beginTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(execTime.AddMinutes(-1 * Interval)), string.Format("{0}00", execTime.AddMinutes(-1 * Interval).ToString("HHmm"))).Value;
                var endTime = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(execTime), string.Format("{0}00", execTime.ToString("HHmm"))).Value;

                Print(beginTime, endTime);
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void Print(DateTime BeginTime, DateTime EndTime)
        {
            try
            {
                Logger.Log(string.Format("Print({0},{1})", DateTimeHelper.DateTime2DateTimeString(BeginTime), DateTimeHelper.DateTime2DateTimeString(EndTime)));

                using (EDbEntities db = new EDbEntities())
                {
                    var repairFormList = db.RForm.Where(x => DateTime.Compare(x.CreateTime, BeginTime) >= 0 && DateTime.Compare(x.CreateTime, EndTime) <= 0).ToList();

                    foreach (var repairForm in repairFormList)
                    {
                        Logger.Log(string.Format("GetDetailViewModel({0})", repairForm.VHNO));

                        RequestResult result = RepairFormDataAccessor.GetDetailViewModel(repairForm.UniqueID);

                        if (result.IsSuccess)
                        {
                            var model = RepairFormDataAccessor.Export(result.Data as DetailViewModel);

                            if (model != null)
                            {
                                Print(model.FullFileName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        private void Print(string Excel)
        {
            try
            {
                Logger.Log(string.Format("Print({0})", Excel));

                ProcessStartInfo psi = new ProcessStartInfo(Excel);

                psi.Verb = "print";
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;

                using (var process = Process.Start(psi))
                {
                    process.Close();
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

        ~PrintHelper()
        {
            Dispose(false);
        }

        #endregion
    }
}
