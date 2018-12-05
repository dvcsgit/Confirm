using DataAccess.ASE.QA;
using Models.ASE.QA.QAPerformance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.Customized_ASE_QA.Controllers
{
    public class QAPerformanceController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryFormModel Model)
        {
            RequestResult result = QAPerformanceHelper.Query(Model.Parameters);

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

        public ActionResult Export()
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = Session["QueryResults"] as List<GridItem>;

                result = QAPerformanceHelper.Export(itemList);

                if (result.IsSuccess)
                {
                    var model = result.Data as ExcelExportModel;

                    var tempPath = Url.Action("Download", "Utils", new { FullFileName = model.FullFileName });

                    return Json(new { success = true, data = tempPath });
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Json(new { errors = GetErrorsFromModelState() });
        }

        protected IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }
    }
}