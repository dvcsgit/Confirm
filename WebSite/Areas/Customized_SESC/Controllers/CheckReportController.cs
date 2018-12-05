#if SESC
using Customized.SESC.DataAccess;
using Customized.SESC.Models.CheckReport;
using Models.Authenticated;
using Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.Customized_SESC.Controllers
{
    public class CheckReportController : Controller
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
            RequestResult result = CheckReportHelper.Query(Model.Parameters);

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
            var model = CheckReportHelper.Export(Session["QueryResults"] as ReportModel, ExcelVersion).Data as ExcelExportModel;

            return File(model.Data, model.ContentType, model.FileName);
        }

        public ActionResult InitTree()
        {
            try
            {
                RequestResult result = CheckReportHelper.GetTreeItem("*", Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_Tree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
                }
                else
                {
                    return PartialView("_Error", result.Error);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult GetTreeItem(string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = CheckReportHelper.GetTreeItem(OrganizationUniqueID, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    jsonTree = JsonConvert.SerializeObject((List<TreeItem>)result.Data);
                }
                else
                {
                    jsonTree = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                jsonTree = string.Empty;
            }

            return Content(jsonTree);
        }
    }
}
#endif