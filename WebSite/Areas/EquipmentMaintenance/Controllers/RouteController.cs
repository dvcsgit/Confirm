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
using Models.EquipmentMaintenance.RouteManagement;

#if ASE
using DataAccess.ASE;
#else
using DataAccess;
using DataAccess.EquipmentMaintenance;
#endif

using System.Web;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class RouteController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = RouteDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = RouteDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
            Session["RouteFormAction"] = Define.EnumFormAction.Create;
            Session["RouteCreateFormModel"] = new CreateFormModel()
            {
                AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(OrganizationUniqueID),
                OrganizationUniqueID = OrganizationUniqueID,
                ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID)
            };

            return PartialView("_Create", Session["RouteCreateFormModel"]);
        }

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = RouteDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["RouteFormAction"] = Define.EnumFormAction.Create;
                Session["RouteCreateFormModel"] = result.Data;

                return PartialView("_Create", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model, string ControlPointPageStates, string ControlPointCheckItemPageStates, string EquipmentPageStates, string EquipmentCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["RouteCreateFormModel"] as CreateFormModel;

                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);
                List<string> equipmentPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentPageStates);
                List<string> equipmentCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentCheckItemPageStates);

                result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList, equipmentPageStateList, equipmentCheckItemPageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.ControlPointList = result.Data as List<ControlPointModel>;

                    result = RouteDataAccessor.Create(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("RouteFormAction");
                        Session.Remove("RouteCreateFormModel");
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
            RequestResult result = RouteDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["RouteFormAction"] = Define.EnumFormAction.Edit;
                Session["RouteEditFormModel"] = result.Data;

                return PartialView("_Edit", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model, string ControlPointPageStates, string ControlPointCheckItemPageStates, string EquipmentPageStates, string EquipmentCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["RouteEditFormModel"] as EditFormModel;

                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);
                List<string> equipmentPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentPageStates);
                List<string> equipmentCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentCheckItemPageStates);

                result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList, equipmentPageStateList, equipmentCheckItemPageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.ControlPointList = result.Data as List<ControlPointModel>;

                    result = RouteDataAccessor.Edit(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("RouteFormAction");
                        Session.Remove("RouteEditFormModel");
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

                result = RouteDataAccessor.Delete(selectedList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

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

        public ActionResult InitDetailTree(string UniqueID)
        {
            try
            {
                RequestResult result = RouteDataAccessor.GetDetailTreeItem(UniqueID);

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

        public ActionResult GetDetailTreeItem(string RouteUniqueID, string ControlPointUniqueID, string EquipmentUniqueID, string PartUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = RouteDataAccessor.GetDetailTreeItem(RouteUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID);

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

        public ActionResult InitControlPointSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = ControlPointDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*");

                if (result.IsSuccess)
                {
                    return PartialView("_ControlPointSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetControlPointSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = ControlPointDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID);

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
                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedList", (Session["RouteCreateFormModel"] as CreateFormModel).ControlPointList);
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedList", (Session["RouteEditFormModel"] as EditFormModel).ControlPointList);
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

        public ActionResult AddControlPoint(string Selecteds, string ControlPointPageStates, string ControlPointCheckItemPageStates, string EquipmentPageStates, string EquipmentCheckItemPageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);
                List<string> equipmentPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentPageStates);
                List<string> equipmentCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentCheckItemPageStates);

                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RouteCreateFormModel"] as CreateFormModel;

                    result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList, equipmentPageStateList, equipmentCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.ControlPointList = result.Data as List<ControlPointModel>;

                        result = RouteDataAccessor.AddControlPoint(model.ControlPointList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.ControlPointList = result.Data as List<ControlPointModel>;

                            Session["RouteCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RouteEditFormModel"] as EditFormModel;

                    result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList, equipmentPageStateList, equipmentCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.ControlPointList = result.Data as List<ControlPointModel>;

                        result = RouteDataAccessor.AddControlPoint(model.ControlPointList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.ControlPointList = result.Data as List<ControlPointModel>;

                            Session["RouteEditFormModel"] = model;
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

        public ActionResult DeleteControlPoint(string ControlPointUniqueID, string ControlPointPageStates, string ControlPointCheckItemPageStates, string EquipmentPageStates, string EquipmentCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);
                List<string> equipmentPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentPageStates);
                List<string> equipmentCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentCheckItemPageStates);

                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RouteCreateFormModel"] as CreateFormModel;

                    result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList, equipmentPageStateList, equipmentCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.ControlPointList = result.Data as List<ControlPointModel>;

                        model.ControlPointList.Remove(model.ControlPointList.First(x => x.UniqueID == ControlPointUniqueID));

                        Session["RouteCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RouteEditFormModel"] as EditFormModel;

                    result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList, equipmentPageStateList, equipmentCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.ControlPointList = result.Data as List<ControlPointModel>;

                        model.ControlPointList.Remove(model.ControlPointList.First(x => x.UniqueID == ControlPointUniqueID));

                        Session["RouteEditFormModel"] = model;

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

        public ActionResult AddEquipment(string ControlPointUniqueID, string Selecteds, string ControlPointPageStates, string ControlPointCheckItemPageStates, string EquipmentPageStates, string EquipmentCheckItemPageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);
                List<string> equipmentPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentPageStates);
                List<string> equipmentCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentCheckItemPageStates);

                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RouteCreateFormModel"] as CreateFormModel;

                    result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList, equipmentPageStateList, equipmentCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.ControlPointList = result.Data as List<ControlPointModel>;

                        result = RouteDataAccessor.AddEquipment(model.ControlPointList, ControlPointUniqueID, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.ControlPointList = result.Data as List<ControlPointModel>;

                            Session["RouteCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RouteEditFormModel"] as EditFormModel;

                    result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList, equipmentPageStateList, equipmentCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.ControlPointList = result.Data as List<ControlPointModel>;

                        result = RouteDataAccessor.AddEquipment(model.ControlPointList, ControlPointUniqueID, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.ControlPointList = result.Data as List<ControlPointModel>;

                            Session["RouteEditFormModel"] = model;
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

        public ActionResult DeleteEquipment(string ControlPointUniqueID, string EquipmentUniqueID, string PartUniqueID, string ControlPointPageStates, string ControlPointCheckItemPageStates, string EquipmentPageStates, string EquipmentCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);
                List<string> equipmentPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentPageStates);
                List<string> equipmentCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentCheckItemPageStates);

                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RouteCreateFormModel"] as CreateFormModel;

                    result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList, equipmentPageStateList, equipmentCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.ControlPointList = result.Data as List<ControlPointModel>;

                        model.ControlPointList.First(x => x.UniqueID == ControlPointUniqueID).EquipmentList.Remove(model.ControlPointList.First(x => x.UniqueID == ControlPointUniqueID).EquipmentList.First(x => x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == PartUniqueID));

                        Session["RouteCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RouteEditFormModel"] as EditFormModel;

                    result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList, equipmentPageStateList, equipmentCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.ControlPointList = result.Data as List<ControlPointModel>;

                        model.ControlPointList.First(x => x.UniqueID == ControlPointUniqueID).EquipmentList.Remove(model.ControlPointList.First(x => x.UniqueID == ControlPointUniqueID).EquipmentList.First(x => x.EquipmentUniqueID == EquipmentUniqueID && x.PartUniqueID == PartUniqueID));

                        Session["RouteEditFormModel"] = model;

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
                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedUserList", (Session["RouteCreateFormModel"] as CreateFormModel).ManagerList);
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedUserList", (Session["RouteEditFormModel"] as EditFormModel).ManagerList);
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

                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RouteCreateFormModel"] as CreateFormModel;

                    result = RouteDataAccessor.AddUser(model.ManagerList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.ManagerList = result.Data as List<ManagerModel>;

                        Session["RouteCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RouteEditFormModel"] as EditFormModel;

                    result = RouteDataAccessor.AddUser(model.ManagerList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.ManagerList = result.Data as List<ManagerModel>;

                        Session["RouteEditFormModel"] = model;
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
                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RouteCreateFormModel"] as CreateFormModel;

                    model.ManagerList.Remove(model.ManagerList.First(x => x.ID == UserID));

                    Session["RouteCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RouteEditFormModel"] as EditFormModel;

                    model.ManagerList.Remove(model.ManagerList.First(x => x.ID == UserID));

                    Session["RouteEditFormModel"] = model;

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
    }
}