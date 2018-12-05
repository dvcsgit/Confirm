using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.Models;
using Utility;

#if ASE
using DataAccess.ASE;
#else
using DataAccess.EquipmentMaintenance;
using DataAccess;
#endif

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class DashboardController : Controller
    {
        // GET: EquipmentMaintenance/Dashboard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetRepairFormList()
        {
            RequestResult result = HomeIndexHelper.GetRepairFormList(Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_RepairFormList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult GetRepairFormDetailList()
        {
            RequestResult result = RepairFormDataAccessor.Query(new Models.EquipmentMaintenance.RepairFormManagement.QueryParameters()
            {
                OrganizationUniqueID = "*",
            }, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_RepairFormDetailList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult GetMaintenanceFormList()
        {
            RequestResult result = HomeIndexHelper.GetMaintenanceFormList(Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_MaintenanceFormList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult GetMaintenanceFormDetailList()
        {
            var accountList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = MaintenanceFormDataAccessor.Query(new Models.EquipmentMaintenance.MaintenanceFormManagement.QueryParameters()
            {
                OrganizationUniqueID = "*",
            }, accountList, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_MaintenanceFormDetailList", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }
    }

    
}