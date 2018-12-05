using DataAccess;
using Models.Authenticated;
using Models.Shared;
using Newtonsoft.Json;
using Report.EquipmentMaintenance.DataAccess;
using Report.EquipmentMaintenance.Models.UnArriveRouteAnalyze;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.EquipmentMaintenanceReport.Controllers
{
    public class UnArriveRouteAnalyzeController : Controller
    {
        // GET: EquipmentMaintenanceReport/UnArriveRouteAnalyze
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
            RequestResult result = UnArriveRouteAnalyzeDataAccessor.Query(Model.Parameters, Session["Account"] as Account);

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
            var excel = UnArriveRouteAnalyzeDataAccessor.Export(Session["QueryResults"] as GridViewModel, ExcelVersion);

            return File(excel.Data, excel.ContentType, excel.FileName);
        }

        public ActionResult InitTree()
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = OrganizationDataAccessor.GetTreeItem(organizationList, "*", Session["Account"] as Account);

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
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = OrganizationDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, Session["Account"] as Account);

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