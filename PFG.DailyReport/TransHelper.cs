using Customized.PFG.DataAccess;
using Customized.PFG.Models.DailyReport;
using DbEntity.MSSQL;
using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Models;

namespace PFG.DailyReport
{
    public class TransHelper : IDisposable
    {
        public void Trans()
        {
            var checkDate = DateTime.Today.AddDays(-1);

            using (EDbEntities db = new EDbEntities())
            {
                var routeList = db.Route.ToList();
                using (DbEntities odb = new DbEntities())
                {
                    foreach (var route in routeList)
                    {
                        RequestResult result = DailyReportHelper.Query(new QueryParameters()
                        {
                            DateString = DateTimeHelper.DateTime2DateStringWithSeperator(checkDate),
                            RouteUniqueID = route.UniqueID
                        });

                        var organization = odb.Organization.FirstOrDefault(x => x.UniqueID == route.OrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            var model = DailyReportHelper.Export(result.Data as ReportModel, Define.EnumExcelVersion._2007).Data as ExcelExportModel;
                            if (model != null)
                            {
                                GenerateReport(model, checkDate, organization != null ? organization.Description : string.Empty);
                            }
                        }
                    }
                }
            }
        }


        private RequestResult GenerateReport(ExcelExportModel model,DateTime checkDate,string facName)
        {
            RequestResult result = new RequestResult();
            try
            {
                var ext = System.IO.Path.GetExtension(model.FullFileName);

                string fileName = string.Format("{0}_{1}{2}", DateTimeHelper.DateTime2DateStringWithSeperator(checkDate), facName, ext);

                if (!System.IO.Directory.Exists(Config.ReportFolder))
                {
                    System.IO.Directory.CreateDirectory(Config.ReportFolder);
                }

                System.IO.File.Copy(model.FullFileName, System.IO.Path.Combine(Config.ReportFolder, fileName), true);
            }
            catch (Exception ex)
            {
                var err = new Error(System.Reflection.MethodBase.GetCurrentMethod(), ex);

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
