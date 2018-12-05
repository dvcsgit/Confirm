#if GuardPatrol
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Models.GuardPatrol.JobManagement;
using DataAccess.GuardPatrol;
using Utility.Models;
using Models.Authenticated;
using Utility;
using DataAccess;
using System.Reflection;
using Newtonsoft.Json;
using Models.Shared;

namespace WebSite.Areas.GuardPatrol.Controllers
{
    public class JobController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult InitTree()
        {
            try
            {
                RequestResult result = JobDataAccessor.GetTreeItem("*", Session["Account"] as Account);

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
                RequestResult result = JobDataAccessor.GetTreeItem(OrganizationUniqueID, Session["Account"] as Account);

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

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = JobDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = JobDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_Detail", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult InitDetailTree(string JobUniqueID, string RouteUniqueID)
        {
            try
            {
                RequestResult result = JobDataAccessor.GetDetailTreeItem(JobUniqueID, RouteUniqueID);

                if (result.IsSuccess)
                {
                    return PartialView("_DetailTree", result.Data);
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

        public ActionResult GetDetailTreeItem(string JobUniqueID, string RouteUniqueID, string ControlPointUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = JobDataAccessor.GetDetailTreeItem(JobUniqueID, RouteUniqueID, ControlPointUniqueID);

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

        [HttpGet]
        public ActionResult Create(string OrganizationUniqueID)
        {
            Session["JobFormAction"] = Define.EnumFormAction.Create;
            Session["JobCreateFormModel"] = new CreateFormModel()
            {
                AncestorOrganizationUniqueID = OrganizationDataAccessor.GetAncestorOrganizationUniqueID(OrganizationUniqueID),
                OrganizationUniqueID = OrganizationUniqueID,
                ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID),
            };

            return PartialView("_Create", Session["JobCreateFormModel"]);
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model, string RoutePageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["JobCreateFormModel"] as CreateFormModel;

                List<string> routePageStateList = JsonConvert.DeserializeObject<List<string>>(RoutePageStates);

                result = JobDataAccessor.SavePageState(model.RouteList, routePageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.RouteList = result.Data as List<RouteModel>;

                    result = JobDataAccessor.Create(model);

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

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = JobDataAccessor.GetCopyFormModel(UniqueID);

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

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = JobDataAccessor.GetEditFormModel(UniqueID);

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
        public ActionResult Edit(EditFormModel Model, string RoutePageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["JobEditFormModel"] as EditFormModel;

                List<string> routePageStateList = JsonConvert.DeserializeObject<List<string>>(RoutePageStates);

                result = JobDataAccessor.SavePageState(model.RouteList, routePageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.RouteList = result.Data as List<RouteModel>;

                    result = JobDataAccessor.Edit(model);

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

        public ActionResult InitUserSelectTree(string AncestorOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = UserDataAccessor.GetRootTreeItem(organizationList, AncestorOrganizationUniqueID, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_UserSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetUserSelectTreeItem(string OrganizationUniqueID)
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

        public ActionResult AddUser(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["JobCreateFormModel"] as CreateFormModel;

                    result = JobDataAccessor.AddUser(model.UserList, selectedList, Session["Account"] as Account);

                    if (result.IsSuccess)
                    {
                        model.UserList = result.Data as List<Models.GuardPatrol.JobManagement.UserModel>;

                        Session["JobCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["JobEditFormModel"] as EditFormModel;

                    result = JobDataAccessor.AddUser(model.UserList, selectedList, Session["Account"] as Account);

                    if (result.IsSuccess)
                    {
                        model.UserList = result.Data as List<Models.GuardPatrol.JobManagement.UserModel>;

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

        public ActionResult GetSelectedRouteList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedRouteList", (Session["JobCreateFormModel"] as CreateFormModel).RouteList);
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedRouteList", (Session["JobEditFormModel"] as EditFormModel).RouteList);
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

        public ActionResult InitRouteSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                RequestResult result = RouteDataAccessor.GetTreeItem(RefOrganizationUniqueID, "*");

                if (result.IsSuccess)
                {
                    return PartialView("_RouteSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetRouteSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = RouteDataAccessor.GetTreeItem(RefOrganizationUniqueID, OrganizationUniqueID);

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

        public ActionResult AddRoute(string Selecteds, string RoutePageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> routePageStateList = JsonConvert.DeserializeObject<List<string>>(RoutePageStates);

                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["JobCreateFormModel"] as CreateFormModel;

                    result = JobDataAccessor.SavePageState(model.RouteList, routePageStateList);

                    if (result.IsSuccess)
                    {
                        model.RouteList = result.Data as List<RouteModel>;

                        result = JobDataAccessor.AddRoute(model.RouteList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.RouteList = result.Data as List<RouteModel>;

                            Session["JobCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["JobEditFormModel"] as EditFormModel;

                    result = JobDataAccessor.SavePageState(model.RouteList, routePageStateList);

                    if (result.IsSuccess)
                    {
                        model.RouteList = result.Data as List<RouteModel>;

                        result = JobDataAccessor.AddRoute(model.RouteList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.RouteList = result.Data as List<RouteModel>;

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

        public ActionResult DeleteRoute(string RouteUniqueID, string RoutePageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> routePageStateList = JsonConvert.DeserializeObject<List<string>>(RoutePageStates);

                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["JobCreateFormModel"] as CreateFormModel;

                    result = JobDataAccessor.SavePageState(model.RouteList, routePageStateList);

                    if (result.IsSuccess)
                    {
                        model.RouteList = result.Data as List<RouteModel>;

                        model.RouteList.Remove(model.RouteList.First(x => x.UniqueID == RouteUniqueID));

                        Session["JobCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["JobEditFormModel"] as EditFormModel;

                    result = JobDataAccessor.SavePageState(model.RouteList, routePageStateList);

                    if (result.IsSuccess)
                    {
                        model.RouteList = result.Data as List<RouteModel>;

                        model.RouteList.Remove(model.RouteList.First(x => x.UniqueID == RouteUniqueID));

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

        [HttpGet]
        public ActionResult EditRoute(string RouteUniqueID)
        {
            try
            {
                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_EditRoute", (Session["JobCreateFormModel"] as CreateFormModel).RouteList.First(x => x.UniqueID == RouteUniqueID));
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_EditRoute", (Session["JobEditFormModel"] as EditFormModel).RouteList.First(x => x.UniqueID == RouteUniqueID));
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

        [HttpPost]
        public ActionResult EditRoute(string RouteUniqueID, string RoutePageStates, string ControlPointPageStates, string ControlPointCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> routePageStateList = JsonConvert.DeserializeObject<List<string>>(RoutePageStates);
                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);

                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["JobCreateFormModel"] as CreateFormModel;

                    result = JobDataAccessor.SavePageState(model.RouteList, routePageStateList);

                    if (result.IsSuccess)
                    {
                        model.RouteList = result.Data as List<RouteModel>;

                        result = JobDataAccessor.SavePageState(model.RouteList, RouteUniqueID, controlPointPageStateList, controlPointCheckItemPageStateList);

                        if (result.IsSuccess)
                        {
                            model.RouteList = result.Data as List<RouteModel>;

                            Session["JobCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["JobEditFormModel"] as EditFormModel;

                    result = JobDataAccessor.SavePageState(model.RouteList, routePageStateList);

                    if (result.IsSuccess)
                    {
                        model.RouteList = result.Data as List<RouteModel>;

                        result = JobDataAccessor.SavePageState(model.RouteList, RouteUniqueID, controlPointPageStateList, controlPointCheckItemPageStateList);

                        if (result.IsSuccess)
                        {
                            model.RouteList = result.Data as List<RouteModel>;

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

        public ActionResult Delete(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                result = JobDataAccessor.Delete(selectedList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }
    }
}
#endif