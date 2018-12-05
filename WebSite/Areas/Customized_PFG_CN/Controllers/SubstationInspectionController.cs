using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;
using Customized.PFG.CN.Models;
using Customized.PFG.CN.DataAccess;
using Customized.PFG.CN.Models.SubstationInspection;

namespace WebSite.Areas.Customized_PFG_CN.Controllers
{
    public class SubstationInspectionController : Controller
    {
        // GET: Customized_PFG_CN/SubstationInspection
        public ActionResult Index()
        {

            return View();

        }
        public ActionResult Export(Define.EnumExcelVersion ExcelVersion)
        {

            var x = 1;

            QueryParameters parameter = Session["parameter"] as QueryParameters;

            string year = parameter.Year;

            var model = SubstationInspection.Export(parameter, ExcelVersion).Data as ExcelExportModel;

            return File(model.Data, model.ContentType, model.FileName);

        }

        public string DateValue(string Year, string Month, string Execl)
        {
            QueryParameters parameters = new QueryParameters();

            parameters.Year = Year;
            parameters.Month = Month;

            Session["parameter"] = parameters;
            return Execl;
        }

        public string Validate(string Year, string Month)
        {
            string prompt = SubstationInspection.Validate(Year, Month);

            return prompt;
        }
    }
}