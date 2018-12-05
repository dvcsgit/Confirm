using System;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.ProgressQuery;

#if ASE
using DataAccess.ASE;
#else
using DataAccess.EquipmentMaintenance;
using DataAccess;
#endif

using System.Web;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
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
            RequestResult result = ProgressQueryHelper.Query(Model.Parameters, Session["Account"] as Account);

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
                var account = Session["Account"] as Account;

                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = new RequestResult();

                if (account.UserRootOrganizationUniqueID == "*")
                {
                    result = RouteDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, "", Session["Account"] as Account);
                }
                else
                {
                    result = RouteDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
                }

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

                RequestResult result = RouteDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, RouteUniqueID, Session["Account"] as Account);

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
