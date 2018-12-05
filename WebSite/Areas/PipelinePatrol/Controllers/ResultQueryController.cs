#if PipelinePatrol
using DataAccess.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.ResultQuery;
using Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.PipelinePatrol.Controllers
{
    public class ResultQueryController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel()
            {
                Parameters = new QueryParameters()
                {
                    BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
                    EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today)
                }
            });
        }

        public ActionResult Query(QueryFormModel Model)
        {
            RequestResult result = ResultQueryHelper.Query(Model.Parameters, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["QueryResults"] = result.Data;

                return PartialView("_JobList", Session["QueryResults"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Detail(string JobUniqueID, string CheckDate, string RouteUniqueID, string PipePointUniqueID)
        {
            try
            {
                if (!string.IsNullOrEmpty(PipePointUniqueID))
                {
                    return PartialView("_CheckItemList", (Session["QueryResults"] as List<JobModel>).First(x => x.UniqueID == JobUniqueID && x.CheckDate == CheckDate).RouteList.First(x => x.UniqueID == RouteUniqueID).PipePointList.First(x => x.UniqueID == PipePointUniqueID));
                }
                else if (!string.IsNullOrEmpty(JobUniqueID))
                {
                    return PartialView("_ControlPointList", (Session["QueryResults"] as List<JobModel>).First(x => x.UniqueID == JobUniqueID && x.CheckDate == CheckDate));
                }
                else
                {
                    return PartialView("_JobList", Session["QueryResults"]);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult InitTree()
        {
            try
            {
                RequestResult result = ResultQueryHelper.GetTreeItem("*", Session["Account"] as Account);

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
                RequestResult result = ResultQueryHelper.GetTreeItem(OrganizationUniqueID, Session["Account"] as Account);

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

        public ActionResult Export(Define.EnumExcelVersion ExcelVersion)
        {
            var model = ResultQueryHelper.Export(Session["QueryResults"] as List<JobModel>, ExcelVersion).Data as ExcelExportModel;

            return File(model.Data, model.ContentType, model.FileName);
        }
    }
}
#endif