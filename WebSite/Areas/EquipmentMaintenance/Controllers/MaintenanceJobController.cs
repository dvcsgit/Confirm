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
using Models.EquipmentMaintenance.MaintenanceJobManagement;

#if ASE
using DataAccess.ASE;
#else
using DataAccess;
using DataAccess.EquipmentMaintenance;
#endif

using System.Web;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class MaintenanceJobController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = MaintenanceJobDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = MaintenanceJobDataAccessor.GetDetailViewModel(UniqueID);

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
        public ActionResult Create(string OrganizationUniqueID)
        {
            Session["JobFormAction"] = Define.EnumFormAction.Create;
            Session["JobCreateFormModel"] = new CreateFormModel()
            {
                AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(OrganizationUniqueID),
                OrganizationUniqueID = OrganizationUniqueID,
                ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID)
            };

            return PartialView("_Create", Session["JobCreateFormModel"]);
        }

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = MaintenanceJobDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["JobFormAction"] = Define.EnumFormAction.Create;
                Session["JobCreateFormModel"] = result.Data;

                return PartialView("_Create", Session["JobCreateFormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model, string EquipmentStandardPageStates, string EquipmentMaterialPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["JobCreateFormModel"] as CreateFormModel;

                List<string> equipmentStandardPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentStandardPageStates);
                List<string> equipmentMaterialPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentMaterialPageStates);

                result = MaintenanceJobDataAccessor.SavePageState(model.EquipmentList, equipmentStandardPageStateList, equipmentMaterialPageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.EquipmentList = result.Data as List<EquipmentModel>;

                    result = MaintenanceJobDataAccessor.Create(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("JobCreateFormModel");
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

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = MaintenanceJobDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["JobFormAction"] = Define.EnumFormAction.Edit;
                Session["JobEditFormModel"] = result.Data;

                return PartialView("_Edit", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model, string EquipmentStandardPageStates, string EquipmentMaterialPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["JobEditFormModel"] as EditFormModel;

                List<string> equipmentStandardPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentStandardPageStates);
                List<string> equipmentMaterialPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentMaterialPageStates);

                result = MaintenanceJobDataAccessor.SavePageState(model.EquipmentList, equipmentStandardPageStateList, equipmentMaterialPageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.EquipmentList = result.Data as List<EquipmentModel>;

                    result = MaintenanceJobDataAccessor.Edit(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("JobEditFormModel");
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

                result = MaintenanceJobDataAccessor.Delete(selectedList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult InitDetailTree(string UniqueID)
        {
            try
            {
                RequestResult result = MaintenanceJobDataAccessor.GetDetailTreeItem(UniqueID);

                if (result.IsSuccess)
                {
                    return PartialView("_DetailTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetDetailTreeItem(string JobUniqueID, string EquipmentUniqueID, string PartUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = MaintenanceJobDataAccessor.GetDetailTreeItem(JobUniqueID, EquipmentUniqueID, PartUniqueID);

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

        public ActionResult InitSelectUserTree(string AncestorOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = UserDataAccessor.GetRootTreeItem(organizationList, AncestorOrganizationUniqueID, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_SelectUserTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSelectUserTreeItem(string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = UserDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, Session["Account"] as Account);

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
                jsonTree = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return Content(jsonTree);
        }

        public ActionResult GetSelectedUserList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedUserList", (Session["JobCreateFormModel"] as CreateFormModel).UserList);
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedUserList", (Session["JobEditFormModel"] as EditFormModel).UserList);
                }
                else
                {
                    return PartialView("_Error", new Error(MethodInfo.GetCurrentMethod(), Resources.Resource.UnKnownOperation));
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult AddUser(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["JobCreateFormModel"] as CreateFormModel;

                    result = MaintenanceJobDataAccessor.AddUser(model.UserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.UserList = result.Data as List<Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel>;

                        Session["JobCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["JobEditFormModel"] as EditFormModel;

                    result = MaintenanceJobDataAccessor.AddUser(model.UserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.UserList = result.Data as List<Models.EquipmentMaintenance.MaintenanceJobManagement.UserModel>;

                        Session["JobEditFormModel"] = model;
                    }
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UnKnownOperation);
                }
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult DeleteUser(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["JobCreateFormModel"] as CreateFormModel;

                    model.UserList.Remove(model.UserList.First(x => x.ID == UserID));

                    Session["JobCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["JobEditFormModel"] as EditFormModel;

                    model.UserList.Remove(model.UserList.First(x => x.ID == UserID));

                    Session["JobEditFormModel"] = model;

                    result.Success();
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UnKnownOperation);
                }
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult InitEquipmentSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = EquipmentDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*");

                if (result.IsSuccess)
                {
                    return PartialView("_EquipmentSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetEquipmentSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = EquipmentDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID);

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

        public ActionResult GetSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedList", (Session["JobCreateFormModel"] as CreateFormModel).EquipmentList);
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedList", (Session["JobEditFormModel"] as EditFormModel).EquipmentList);
                }
                else
                {
                    return PartialView("_Error", new Error(MethodBase.GetCurrentMethod(), Resources.Resource.UnKnownOperation));
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult AddEquipment(string Selecteds, string EquipmentStandardPageStates, string EquipmentMaterialPageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> equipmentStandardPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentStandardPageStates);
                List<string> equipmentMaterialPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentMaterialPageStates);

                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["JobCreateFormModel"] as CreateFormModel;

                    result = MaintenanceJobDataAccessor.SavePageState(model.EquipmentList, equipmentStandardPageStateList, equipmentMaterialPageStateList);

                    if (result.IsSuccess)
                    {
                        model.EquipmentList = result.Data as List<EquipmentModel>;

                        result = MaintenanceJobDataAccessor.AddEquipment(model.EquipmentList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.EquipmentList = result.Data as List<EquipmentModel>;

                            Session["JobCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["JobEditFormModel"] as EditFormModel;

                    result = MaintenanceJobDataAccessor.SavePageState(model.EquipmentList, equipmentStandardPageStateList, equipmentMaterialPageStateList);

                    if (result.IsSuccess)
                    {
                        model.EquipmentList = result.Data as List<EquipmentModel>;

                        result = MaintenanceJobDataAccessor.AddEquipment(model.EquipmentList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.EquipmentList = result.Data as List<EquipmentModel>;

                            Session["JobEditFormModel"] = model;
                        }
                    }
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UnKnownOperation);
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

        public ActionResult DeleteEquipment(string EquipmentUniqueID, string PartUniqueID, string EquipmentStandardPageStates, string EquipmentMaterialPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> equipmentStandardPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentStandardPageStates);
                List<string> equipmentMaterialPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentMaterialPageStates);

                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["JobCreateFormModel"] as CreateFormModel;

                    result = MaintenanceJobDataAccessor.SavePageState(model.EquipmentList, equipmentStandardPageStateList, equipmentMaterialPageStateList);

                    if (result.IsSuccess)
                    {
                        model.EquipmentList = result.Data as List<EquipmentModel>;

                        model.EquipmentList.Remove(model.EquipmentList.First(x => x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == PartUniqueID));

                        Session["JobCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["JobEditFormModel"] as EditFormModel;

                    result = MaintenanceJobDataAccessor.SavePageState(model.EquipmentList, equipmentStandardPageStateList, equipmentMaterialPageStateList);

                    if (result.IsSuccess)
                    {
                        model.EquipmentList = result.Data as List<EquipmentModel>;

                        model.EquipmentList.Remove(model.EquipmentList.First(x => x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == PartUniqueID));

                        Session["JobEditFormModel"] = model;

                        result.Success();
                    }
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UnKnownOperation);
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

        public ActionResult InitTree()
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                var account = Session["Account"] as Account;

                RequestResult result = new RequestResult();

                if (account.UserRootOrganizationUniqueID == "*")
                {
                    result = MaintenanceJobDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
                }
                else
                {
                    result = MaintenanceJobDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
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

        public ActionResult GetTreeItem(string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = MaintenanceJobDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, Session["Account"] as Account);

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