#if CHIMEI
using DataAccess;
using Customized.CHIMEI.DataAccess;
using Models.EquipmentMaintenance.ProgressQuery;
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

namespace WebSite.Areas.Customized_CHIMEI.Controllers
{
    [AllowAnonymous]
    public class ProgressQueryController : Controller
    {
        public ActionResult Index(string JobResultUniqueID)
        {
            return View(new QueryFormModel()
            {
                Parameters = new QueryParameters()
                {
                    JobResultUniqueID = JobResultUniqueID,
                    BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
                    EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today)
                }
            });
        }

        public ActionResult Query(QueryFormModel Model)
        {
            RequestResult result = ProgressQueryHelper.Query(Model.Parameters);

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

        public ActionResult Detail(string JobResultUniqueID, string ControlPointUniqueID)
        {
            try
            {
                if (!string.IsNullOrEmpty(ControlPointUniqueID))
                {
                    return PartialView("_CheckItemList", (Session["QueryResults"] as List<JobResultModel>).First(x => x.UniqueID == JobResultUniqueID).ControlPointList.First(x => x.UniqueID == ControlPointUniqueID));
                }
                else if (!string.IsNullOrEmpty(JobResultUniqueID))
                {
                    return PartialView("_ControlPointList", (Session["QueryResults"] as List<JobResultModel>).First(x => x.UniqueID == JobResultUniqueID));
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

        public ActionResult Export(Define.EnumExcelVersion ExcelVersion)
        {
            var model = ProgressQueryHelper.Export(Session["QueryResults"] as List<JobResultModel>, ExcelVersion).Data as ExcelExportModel;

            return File(model.Data, model.ContentType, model.FileName);
        }

        public ActionResult InitTree()
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = new RequestResult();

                result = ProgressQueryHelper.GetTreeItem(organizationList, "*", "");

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

        public ActionResult GetTreeItem(string OrganizationUniqueID, string RouteUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = ProgressQueryHelper.GetTreeItem(organizationList, OrganizationUniqueID, RouteUniqueID);

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