using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.EquipmentStandardManagement;

#if ASE
using DataAccess.ASE;
#else
using DataAccess.EquipmentMaintenance;
using DataAccess;
#endif

using System.Web;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class EquipmentStandardController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = EquipmentStandardDataAccessor.Query(Parameters, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_List", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Detail(string UniqueID)
        {
            RequestResult result = EquipmentStandardDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = EquipmentStandardDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["EquipmentStandardEditFormModel"] = result.Data;

                return PartialView("_Edit", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model, string StandardPageStates, string PartPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EquipmentStandardEditFormModel"] as EditFormModel;

                var standardPageStateList = new List<string>();

                if (!string.IsNullOrEmpty(StandardPageStates))
                {
                    standardPageStateList = JsonConvert.DeserializeObject<List<string>>(StandardPageStates);
                }

                result = EquipmentStandardDataAccessor.SavePageState(model.StandardList, standardPageStateList);

                if (result.IsSuccess)
                {
                    model.StandardList = result.Data as List<StandardModel>;

                    var partPageStateList = new List<string>();

                    if (!string.IsNullOrEmpty(PartPageStates))
                    {
                        partPageStateList = JsonConvert.DeserializeObject<List<string>>(PartPageStates);
                    }

                    result = EquipmentStandardDataAccessor.SavePageState(model.PartList, partPageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        result = EquipmentStandardDataAccessor.Edit(model);

                        if (result.IsSuccess)
                        {
                            Session.Remove("EquipmentStandardEditFormModel");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult Delete(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                result = EquipmentDataAccessor.Delete(selectedList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult InitStandardSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = StandardDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_StandardSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult InitPartStandardSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = StandardDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_PartStandardSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetStandardSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string MaintenanceType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = StandardDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, MaintenanceType);

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

        public ActionResult GetStandardSelectedList()
        {
            try
            {
                return PartialView("_StandardSelectedList", Session["EquipmentStandardEditFormModel"] as EditFormModel);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult GetPartList()
        {
            try
            {
                return PartialView("_PartList", (Session["EquipmentStandardEditFormModel"] as EditFormModel).PartList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult AddStandard(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                var model = Session["EquipmentStandardEditFormModel"] as EditFormModel;

                result = EquipmentStandardDataAccessor.SavePageState(model.StandardList, pageStateList);

                if (result.IsSuccess)
                {
                    model.StandardList = result.Data as List<StandardModel>;

                    result = EquipmentStandardDataAccessor.AddStandard(model.StandardList, selectedList, RefOrganizationUniqueID);

                    if (result.IsSuccess)
                    {
                        model.StandardList = result.Data as List<StandardModel>;

                        Session["EquipmentStandardEditFormModel"] = model;
                    }
                }
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult DeleteStandard(string StandardUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                var model = Session["EquipmentStandardEditFormModel"] as EditFormModel;

                result = EquipmentStandardDataAccessor.SavePageState(model.StandardList, pageStateList);

                if (result.IsSuccess)
                {
                    model.StandardList = result.Data as List<StandardModel>;

                    model.StandardList.Remove(model.StandardList.First(x => x.UniqueID == StandardUniqueID));

                    Session["EquipmentStandardEditFormModel"] = model;

                    result.Success();
                }
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult AddPartStandard(string PartUniqueID, string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                var model = Session["EquipmentStandardEditFormModel"] as EditFormModel;

                result = EquipmentStandardDataAccessor.SavePageState(model.PartList, pageStateList);

                if (result.IsSuccess)
                {
                    model.PartList = result.Data as List<PartModel>;

                    var part = model.PartList.First(x => x.UniqueID == PartUniqueID);

                    result = EquipmentStandardDataAccessor.AddStandard(part.StandardList, selectedList, RefOrganizationUniqueID);

                    if (result.IsSuccess)
                    {
                        part.StandardList = result.Data as List<StandardModel>;

                        Session["EquipmentStandardEditFormModel"] = model;
                    }
                }
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult DeletePartStandard(string PartUniqueID, string StandardUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                var model = Session["EquipmentStandardEditFormModel"] as EditFormModel;

                result = EquipmentStandardDataAccessor.SavePageState(model.PartList, pageStateList);

                if (result.IsSuccess)
                {
                    model.PartList = result.Data as List<PartModel>;

                    var part = model.PartList.First(x => x.UniqueID == PartUniqueID);

                    part.StandardList.Remove(part.StandardList.First(x => x.UniqueID == StandardUniqueID));

                    Session["EquipmentStandardEditFormModel"] = model;

                    result.Success();
                }
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }
    }
}