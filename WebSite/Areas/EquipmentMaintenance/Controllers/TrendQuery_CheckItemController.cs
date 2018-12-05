#if ASE
using DataAccess.ASE;
#else
using DataAccess.EquipmentMaintenance;
using DataAccess;
#endif
using Models.Authenticated;
using Models.EquipmentMaintenance.TrendQuery_CheckItem;
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

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class TrendQuery_CheckItemController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Query(string CheckItemUniqueID, string Keyword)
        {
            RequestResult result = TrendQuery_CheckItemHelper.Query(CheckItemUniqueID, Keyword);

            if (result.IsSuccess)
            {
                return PartialView("_List", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Draw(GridViewModel Model)
        {
            return PartialView("_Chart", TrendQuery_CheckItemHelper.Draw(Model.Parameters));
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
                    result = TrendQuery_CheckItemHelper.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, "", Session["Account"] as Account);
                }
                else
                {
                    result = TrendQuery_CheckItemHelper.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
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
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult GetTreeItem(string OrganizationUniqueID, string CheckType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = TrendQuery_CheckItemHelper.GetTreeItem(organizationList, OrganizationUniqueID, CheckType, Session["Account"] as Account);

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
                Logger.Log(MethodInfo.GetCurrentMethod(), ex);

                jsonTree = string.Empty;
            }

            return Content(jsonTree);
        }
    }
}