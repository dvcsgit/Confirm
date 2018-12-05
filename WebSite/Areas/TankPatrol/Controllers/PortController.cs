using DataAccess;
using DataAccess.TankPatrol;
using Models.Authenticated;
using Models.Shared;
using Models.TankPatrol.PortManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.TankPatrol.Controllers
{
    public class PortController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            try
            {
                RequestResult result = PortDataAccessor.Query(Parameters, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_List", result.Data);
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

        [HttpGet]
        public ActionResult Detail(string UniqueID)
        {
            RequestResult result = PortDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
        public ActionResult Create(string OrganizationUniqueID, string StationUniqueID, string IslandUniqueID)
        {
            RequestResult result = PortDataAccessor.GetCreateFormModel(OrganizationUniqueID, StationUniqueID, IslandUniqueID);

            if (result.IsSuccess)
            {
                Session["FormModel"] = result.Data;
                Session["FormAction"] = Define.EnumFormAction.Create;

                return PartialView("_Create", Session["FormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model, string LBPageState, string LPPageState, string LAPageState, string LDPageState, string UBPageState, string UPPageState, string UAPageState, string UDPageState)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["FormModel"] as CreateFormModel;

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(LBPageState);

                result = PortDataAccessor.SavePageState(model.LBCheckItemList, pageStateList);

                if (result.IsSuccess)
                {
                    model.LBCheckItemList = result.Data as List<CheckItemModel>;

                    pageStateList = JsonConvert.DeserializeObject<List<string>>(LPPageState);

                    result = PortDataAccessor.SavePageState(model.LPCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LPCheckItemList = result.Data as List<CheckItemModel>;

                        pageStateList = JsonConvert.DeserializeObject<List<string>>(LAPageState);

                        result = PortDataAccessor.SavePageState(model.LACheckItemList, pageStateList);

                        if (result.IsSuccess)
                        {
                            model.LACheckItemList = result.Data as List<CheckItemModel>;

                            pageStateList = JsonConvert.DeserializeObject<List<string>>(LDPageState);

                            result = PortDataAccessor.SavePageState(model.LDCheckItemList, pageStateList);

                            if (result.IsSuccess)
                            {
                                model.LDCheckItemList = result.Data as List<CheckItemModel>;

                                pageStateList = JsonConvert.DeserializeObject<List<string>>(UBPageState);

                                result = PortDataAccessor.SavePageState(model.UBCheckItemList, pageStateList);

                                if (result.IsSuccess)
                                {
                                    model.UBCheckItemList = result.Data as List<CheckItemModel>;

                                    pageStateList = JsonConvert.DeserializeObject<List<string>>(UPPageState);

                                    result = PortDataAccessor.SavePageState(model.UPCheckItemList, pageStateList);

                                    if (result.IsSuccess)
                                    {
                                        model.UPCheckItemList = result.Data as List<CheckItemModel>;

                                        pageStateList = JsonConvert.DeserializeObject<List<string>>(UAPageState);

                                        result = PortDataAccessor.SavePageState(model.UACheckItemList, pageStateList);

                                        if (result.IsSuccess)
                                        {
                                            model.UACheckItemList = result.Data as List<CheckItemModel>;

                                            pageStateList = JsonConvert.DeserializeObject<List<string>>(UDPageState);

                                            result = PortDataAccessor.SavePageState(model.UDCheckItemList, pageStateList);

                                            if (result.IsSuccess)
                                            {
                                                model.UDCheckItemList = result.Data as List<CheckItemModel>;

                                                model.FormInput = Model.FormInput;

                                                result = PortDataAccessor.Create(model);
                                            }
                                        }
                                    }
                                }
                            }
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

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = PortDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["FormModel"] = result.Data;
                Session["FormAction"] = Define.EnumFormAction.Edit;

                return PartialView("_Edit", Session["FormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model, string LBPageState, string LPPageState, string LAPageState, string LDPageState, string UBPageState, string UPPageState, string UAPageState, string UDPageState)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["FormModel"] as EditFormModel;

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(LBPageState);

                result = PortDataAccessor.SavePageState(model.LBCheckItemList, pageStateList);

                if (result.IsSuccess)
                {
                    model.LBCheckItemList = result.Data as List<CheckItemModel>;

                    pageStateList = JsonConvert.DeserializeObject<List<string>>(LPPageState);

                    result = PortDataAccessor.SavePageState(model.LPCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LPCheckItemList = result.Data as List<CheckItemModel>;

                        pageStateList = JsonConvert.DeserializeObject<List<string>>(LAPageState);

                        result = PortDataAccessor.SavePageState(model.LACheckItemList, pageStateList);

                        if (result.IsSuccess)
                        {
                            model.LACheckItemList = result.Data as List<CheckItemModel>;

                            pageStateList = JsonConvert.DeserializeObject<List<string>>(LDPageState);

                            result = PortDataAccessor.SavePageState(model.LDCheckItemList, pageStateList);

                            if (result.IsSuccess)
                            {
                                model.LDCheckItemList = result.Data as List<CheckItemModel>;

                                pageStateList = JsonConvert.DeserializeObject<List<string>>(UBPageState);

                                result = PortDataAccessor.SavePageState(model.UBCheckItemList, pageStateList);

                                if (result.IsSuccess)
                                {
                                    model.UBCheckItemList = result.Data as List<CheckItemModel>;

                                    pageStateList = JsonConvert.DeserializeObject<List<string>>(UPPageState);

                                    result = PortDataAccessor.SavePageState(model.UPCheckItemList, pageStateList);

                                    if (result.IsSuccess)
                                    {
                                        model.UPCheckItemList = result.Data as List<CheckItemModel>;

                                        pageStateList = JsonConvert.DeserializeObject<List<string>>(UAPageState);

                                        result = PortDataAccessor.SavePageState(model.UACheckItemList, pageStateList);

                                        if (result.IsSuccess)
                                        {
                                            model.UACheckItemList = result.Data as List<CheckItemModel>;

                                            pageStateList = JsonConvert.DeserializeObject<List<string>>(UDPageState);

                                            result = PortDataAccessor.SavePageState(model.UDCheckItemList, pageStateList);

                                            if (result.IsSuccess)
                                            {
                                                model.UDCheckItemList = result.Data as List<CheckItemModel>;

                                                model.FormInput = Model.FormInput;

                                                result = PortDataAccessor.Edit(model);
                                            }
                                        }
                                    }
                                }
                            }
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

        public ActionResult Delete(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(PortDataAccessor.Delete(UniqueID)));
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
                    result = PortDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, "", "", Session["Account"] as Account);
                }
                else
                {
                    result = PortDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
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

        public ActionResult GetTreeItem(string OrganizationUniqueID, string StationUniqueID, string IslandUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = PortDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, StationUniqueID, IslandUniqueID, Session["Account"] as Account);

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

        public ActionResult InitSelectLBTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_LBSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSelectLBTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string CheckType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, CheckType);

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

        public ActionResult GetSelectedLBList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_LBSelectedList", (Session["FormModel"] as CreateFormModel).LBCheckItemList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_LBSelectedList", (Session["FormModel"] as EditFormModel).LBCheckItemList);
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

        public ActionResult AddLB(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.LBCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LBCheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.LBCheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.LBCheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.LBCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LBCheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.LBCheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.LBCheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
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

        public ActionResult DeleteSelectedLB(string CheckItemUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.LBCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LBCheckItemList = result.Data as List<CheckItemModel>;

                        model.LBCheckItemList.Remove(model.LBCheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.LBCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LBCheckItemList = result.Data as List<CheckItemModel>;

                        model.LBCheckItemList.Remove(model.LBCheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

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

        public ActionResult InitSelectLPTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_LPSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSelectLPTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string CheckType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, CheckType);

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

        public ActionResult GetSelectedLPList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_LPSelectedList", (Session["FormModel"] as CreateFormModel).LPCheckItemList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_LPSelectedList", (Session["FormModel"] as EditFormModel).LPCheckItemList);
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

        public ActionResult AddLP(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.LPCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LPCheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.LPCheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.LPCheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.LPCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LPCheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.LPCheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.LPCheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
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

        public ActionResult DeleteSelectedLP(string CheckItemUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.LPCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LPCheckItemList = result.Data as List<CheckItemModel>;

                        model.LPCheckItemList.Remove(model.LPCheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.LPCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LPCheckItemList = result.Data as List<CheckItemModel>;

                        model.LPCheckItemList.Remove(model.LPCheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

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

        public ActionResult InitSelectLATree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_LASelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSelectLATreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string CheckType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, CheckType);

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

        public ActionResult GetSelectedLAList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_LASelectedList", (Session["FormModel"] as CreateFormModel).LACheckItemList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_LASelectedList", (Session["FormModel"] as EditFormModel).LACheckItemList);
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

        public ActionResult AddLA(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.LACheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LACheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.LACheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.LACheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.LACheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LACheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.LACheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.LACheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
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

        public ActionResult DeleteSelectedLA(string CheckItemUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.LACheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LACheckItemList = result.Data as List<CheckItemModel>;

                        model.LACheckItemList.Remove(model.LACheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.LACheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LACheckItemList = result.Data as List<CheckItemModel>;

                        model.LACheckItemList.Remove(model.LACheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

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



        public ActionResult InitSelectLDTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_LDSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSelectLDTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string CheckType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, CheckType);

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

        public ActionResult GetSelectedLDList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_LDSelectedList", (Session["FormModel"] as CreateFormModel).LDCheckItemList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_LDSelectedList", (Session["FormModel"] as EditFormModel).LDCheckItemList);
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

        public ActionResult AddLD(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.LDCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LDCheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.LDCheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.LDCheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.LDCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LDCheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.LDCheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.LDCheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
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

        public ActionResult DeleteSelectedLD(string CheckItemUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.LDCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LDCheckItemList = result.Data as List<CheckItemModel>;

                        model.LDCheckItemList.Remove(model.LDCheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.LDCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.LDCheckItemList = result.Data as List<CheckItemModel>;

                        model.LDCheckItemList.Remove(model.LDCheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

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
       

        public ActionResult InitSelectUBTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_UBSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSelectUBTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string CheckType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, CheckType);

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

        public ActionResult GetSelectedUBList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_UBSelectedList", (Session["FormModel"] as CreateFormModel).UBCheckItemList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_UBSelectedList", (Session["FormModel"] as EditFormModel).UBCheckItemList);
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

        public ActionResult AddUB(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.UBCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UBCheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.UBCheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.UBCheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.UBCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UBCheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.UBCheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.UBCheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
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

        public ActionResult DeleteSelectedUB(string CheckItemUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.UBCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UBCheckItemList = result.Data as List<CheckItemModel>;

                        model.UBCheckItemList.Remove(model.UBCheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.UBCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UBCheckItemList = result.Data as List<CheckItemModel>;

                        model.UBCheckItemList.Remove(model.UBCheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

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

        public ActionResult InitSelectUPTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_UPSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSelectUPTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string CheckType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, CheckType);

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

        public ActionResult GetSelectedUPList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_UPSelectedList", (Session["FormModel"] as CreateFormModel).UPCheckItemList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_UPSelectedList", (Session["FormModel"] as EditFormModel).UPCheckItemList);
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

        public ActionResult AddUP(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.UPCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UPCheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.UPCheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.UPCheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.UPCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UPCheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.UPCheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.UPCheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
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

        public ActionResult DeleteSelectedUP(string CheckItemUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.UPCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UPCheckItemList = result.Data as List<CheckItemModel>;

                        model.UPCheckItemList.Remove(model.UPCheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.UPCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UPCheckItemList = result.Data as List<CheckItemModel>;

                        model.UPCheckItemList.Remove(model.UPCheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

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

        public ActionResult InitSelectUATree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_UASelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSelectUATreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string CheckType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, CheckType);

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

        public ActionResult GetSelectedUAList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_UASelectedList", (Session["FormModel"] as CreateFormModel).UACheckItemList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_UASelectedList", (Session["FormModel"] as EditFormModel).UACheckItemList);
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

        public ActionResult AddUA(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.UACheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UACheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.UACheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.UACheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.UACheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UACheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.UACheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.UACheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
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

        public ActionResult DeleteSelectedUA(string CheckItemUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.UACheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UACheckItemList = result.Data as List<CheckItemModel>;

                        model.UACheckItemList.Remove(model.UACheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.UACheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UACheckItemList = result.Data as List<CheckItemModel>;

                        model.UACheckItemList.Remove(model.UACheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

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


        public ActionResult InitSelectUDTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_UDSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSelectUDTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string CheckType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, CheckType);

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

        public ActionResult GetSelectedUDList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_UDSelectedList", (Session["FormModel"] as CreateFormModel).UDCheckItemList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_UDSelectedList", (Session["FormModel"] as EditFormModel).UDCheckItemList);
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

        public ActionResult AddUD(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.UDCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UDCheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.UDCheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.UDCheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.UDCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UDCheckItemList = result.Data as List<CheckItemModel>;

                        result = PortDataAccessor.AddCheckItem(model.UDCheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.UDCheckItemList = result.Data as List<CheckItemModel>;

                            Session["FormModel"] = model;
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

        public ActionResult DeleteSelectedUD(string CheckItemUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = PortDataAccessor.SavePageState(model.UDCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UDCheckItemList = result.Data as List<CheckItemModel>;

                        model.UDCheckItemList.Remove(model.UDCheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = PortDataAccessor.SavePageState(model.UDCheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.UDCheckItemList = result.Data as List<CheckItemModel>;

                        model.UDCheckItemList.Remove(model.UDCheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["FormModel"] = model;

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
    }
}