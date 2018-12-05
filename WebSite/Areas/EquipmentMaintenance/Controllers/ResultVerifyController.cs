using Models.Authenticated;
using Models.EquipmentMaintenance.ResultVerify;
using Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Utility;
using Utility.Models;

#if ASE
using DataAccess.ASE;
#else
using DataAccess.EquipmentMaintenance;
using DataAccess;
#endif

using System.Web;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class ResultVerifyController : Controller
    {
        public ActionResult Index(string JobResultUniqueID)
        {
            if (!string.IsNullOrEmpty(JobResultUniqueID))
            {
                ViewBag.JobResultUniqueID = JobResultUniqueID;
            }

            return View(new QueryFormModel()
            {
                Parameters = new QueryParameters()
                {
                    BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today.AddDays(-1)),
                    EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today)
                }
            });
        }

        public ActionResult Query(QueryFormModel Model)
        {
            RequestResult result = ResultVerifyHelper.Query(Model.Parameters, Session["Account"] as Account);

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

        public ActionResult Detail(string UniqueID)
        {
            RequestResult result = ResultVerifyHelper.GetDetailViewModel(UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_Detail", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpGet]
        public ActionResult Verify(string UniqueID)
        {
            RequestResult result = ResultVerifyHelper.GetVerifyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_Verify", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

#if ASE
        public ActionResult Confirm(VerifyFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(ResultVerifyHelper.Confirm(Model.UniqueID, Model.FormInput)));
        }
#else
        [HttpPost]
        public ActionResult Confirm(VerifyFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(ResultVerifyHelper.Confirm(Model.UniqueID, Model.FormInput, Session["Account"] as Account)));
        }
#endif
       

        public ActionResult InitTree()
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                var account = Session["Account"] as Account;

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

#if ASE
        public ActionResult CheckResultDetail(string UniqueID)
        {
            RequestResult result = DataAccess.ASE.ProgressQueryHelper.Query(new Models.EquipmentMaintenance.ProgressQuery.QueryParameters()
            {
                JobResultUniqueID = UniqueID
            }, null);

            if (result.IsSuccess)
            {
                return PartialView("_CheckItemList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }
#else
         public ActionResult CheckResultDetail(string UniqueID)
        {
            RequestResult result = DataAccess.EquipmentMaintenance.ProgressQueryHelper.Query(new Models.EquipmentMaintenance.ProgressQuery.QueryParameters()
            {
                JobResultUniqueID = UniqueID
            }, null);

            if (result.IsSuccess)
            {
                return PartialView("_CheckItemList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }
#endif

    }
}