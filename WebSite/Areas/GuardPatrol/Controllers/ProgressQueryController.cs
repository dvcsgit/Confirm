#if GuardPatrol
using DataAccess.GuardPatrol;
using Models.Authenticated;
using Models.GuardPatrol.ProgressQuery;
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

namespace WebSite.Areas.GuardPatrol.Controllers
{
    public class ProgressQueryController : Controller
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
            RequestResult result = ProgressQueryHelper.Query(Model.Parameters, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["QueryResults"] = result.Data;

                return PartialView("_JobRouteList", Session["QueryResults"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Detail(string JobRouteUniqueID, string BeginDate, string EndDate, string ControlPointUniqueID)
        {
            try
            {
                if (!string.IsNullOrEmpty(ControlPointUniqueID))
                {
                    return PartialView("_CheckItemList", (Session["QueryResults"] as List<JobRouteModel>).First(x => x.UniqueID == JobRouteUniqueID && x.BeginDate == BeginDate && x.EndDate == EndDate).ControlPointList.First(x => x.UniqueID == ControlPointUniqueID));
                }
                else if (!string.IsNullOrEmpty(JobRouteUniqueID))
                {
                    return PartialView("_ControlPointList", (Session["QueryResults"] as List<JobRouteModel>).First(x => x.UniqueID == JobRouteUniqueID && x.BeginDate == BeginDate && x.EndDate == EndDate));
                }
                else
                {
                    return PartialView("_JobRouteList", Session["QueryResults"]);
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
                RequestResult result = JobDataAccessor.GetTreeItem("*", Session["Account"] as Account);

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
                RequestResult result = JobDataAccessor.GetTreeItem(OrganizationUniqueID, Session["Account"] as Account);

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
            var model = ProgressQueryHelper.Export(Session["QueryResults"] as List<JobRouteModel>, ExcelVersion).Data as ExcelExportModel;

            return File(model.Data, model.ContentType, model.FileName);
        }
    }
}
#endif