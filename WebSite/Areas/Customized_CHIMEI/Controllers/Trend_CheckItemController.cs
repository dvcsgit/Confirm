#if CHIMEI
using Customized.CHIMEI.DataAccess;
using DataAccess;
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

namespace WebSite.Areas.Customized_CHIMEI.Controllers
{
    [AllowAnonymous]
    public class Trend_CheckItemController : Controller
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
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = new RequestResult();

                result = TrendQuery_CheckItemHelper.GetTreeItem(organizationList, "*", "");

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

                RequestResult result = TrendQuery_CheckItemHelper.GetTreeItem(organizationList, OrganizationUniqueID, CheckType);

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
#endif