using Customized.LCY.DataAccess;
using Customized.LCY.Models.TankDailyReport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.Customized_LCY.Controllers
{
    public class TankDailyReportController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel()
            {
                Parameters = new QueryParameters()
                {
                    DateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today)
                }
            });
        }

        public ActionResult Query(QueryFormModel Model)
        {
            RequestResult result = TankDailyReportHelper.Query(Model.Parameters);

            if (result.IsSuccess)
            {
                Session["QueryResults"] = result.Data;

                return PartialView("_Export", Session["QueryResults"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Export()
        {
            var model = TankDailyReportHelper.Export(Session["QueryResults"] as ReportModel).Data as ExcelExportModel;

            return File(model.Data, model.ContentType, model.FileName);
        }
    }
}