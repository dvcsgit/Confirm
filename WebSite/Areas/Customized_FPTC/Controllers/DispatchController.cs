using Customized.FPTC.DataAccess;
using Customized.FPTC.Models.DispatchQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.Customized_FPTC.Controllers
{
    public class DispatchController : Controller
    {
        public ActionResult Index()
        {
            RequestResult result = DispatchQueryHelper.GetQueryFormModel();

            if (result.IsSuccess)
            {
                return View(result.Data);
            }
            else
            {
                ViewBag.Error = result.Error;

                return View("Error");
            }
        }

        public ActionResult Query(QueryFormModel Model)
        {
            RequestResult result = DispatchQueryHelper.Query(Model.Parameters);

            if (result.IsSuccess)
            {
                Session["QueryResults"] = result.Data;

                return PartialView("_List", Session["QueryResults"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Export(Define.EnumExcelVersion ExcelVersion)
        {
            var model = DispatchQueryHelper.Export(Session["QueryResults"] as List<GridItem>, ExcelVersion).Data as ExcelExportModel;

            return File(model.Data, model.ContentType, model.FileName);
        }
    }
}