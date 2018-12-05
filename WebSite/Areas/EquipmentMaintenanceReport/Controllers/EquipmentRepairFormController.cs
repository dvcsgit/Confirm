using System;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using DataAccess;
using Report.EquipmentMaintenance.DataAccess;
using Report.EquipmentMaintenance.Models.EquipmentRepairForm;
using System.Linq;
using System.Web.Security;
using System.Web;

namespace WebSite.Areas.EquipmentMaintenanceReport.Controllers
{
    public class EquipmentRepairFormController : Controller
    {
        public ActionResult Index(string RepairFormUniqueID, string CheckResultUniqueID,string OrganizationUniqueID)
        {
            RequestResult result = EquipmentRepairFormDataAccessor.GetQueryFormModel(RepairFormUniqueID, CheckResultUniqueID, OrganizationUniqueID);

            if (result.IsSuccess)
            {
                return View(result.Data);
            }
            else
            {
                ViewBag.Error = result.Error;

                return View("Error");
            }
        }

        public ActionResult EuipQuery()
        {
            string OrganizationUniqueID = Request["OrganizationID"].ToString();
            RequestResult result = EquipmentRepairFormDataAccessor.GetEuipmentList(OrganizationUniqueID);
            string jsoneuipmentList = JsonConvert.SerializeObject(result.Data);
            return Json(jsoneuipmentList);
            
        }

        public ActionResult Query(QueryFormModel Model)
        {
            RequestResult result = EquipmentRepairFormDataAccessor.Query(Model.Parameters, Session["Account"] as Account);

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
                      

        public ActionResult InitTree()
        {
            try
            {
                RequestResult result = OrganizationDataAccessor.GetTreeItem("*", Session["Account"] as Account);

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
                RequestResult result = OrganizationDataAccessor.GetTreeItem(OrganizationUniqueID, Session["Account"] as Account);

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
            var model = EquipmentRepairFormDataAccessor.Export(Session["QueryResults"] as GridViewModel, ExcelVersion);

            return File(model.Data, model.ContentType, model.FileName);
        }
    }
}
