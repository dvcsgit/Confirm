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
using Models.EquipmentMaintenance.JobManagement;

#if ASE
using DataAccess.ASE;
#else
using DataAccess;
using DataAccess.EquipmentMaintenance;
#endif

using System.Web;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class JobController : Controller
    {
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

        [HttpGet]
        public ActionResult Create(string RouteUniqueID)
        {
            RequestResult result = JobDataAccessor.GetCreateFormModel(RouteUniqueID);

            if (result.IsSuccess)
            {
                Session["JobFormAction"] = Define.EnumFormAction.Create;
                Session["JobCreateFormModel"] = result.Data as CreateFormModel;

                return PartialView("_Create", Session["JobCreateFormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
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

        [HttpPost]
        public ActionResult Create(CreateFormModel Model, string ControlPointPageStates, string ControlPointCheckItemPageStates, string EquipmentCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["JobCreateFormModel"] as CreateFormModel;

                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);
                List<string> equipmentCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentCheckItemPageStates);

                result = JobDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList, equipmentCheckItemPageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.ControlPointList = result.Data as List<ControlPointModel>;

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
        public ActionResult Edit(EditFormModel Model, string ControlPointPageStates, string ControlPointCheckItemPageStates, string EquipmentCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["JobEditFormModel"] as EditFormModel;

                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);
                List<string> equipmentCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(EquipmentCheckItemPageStates);

                result = JobDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList, equipmentCheckItemPageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.ControlPointList = result.Data as List<ControlPointModel>;

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

        public ActionResult Delete(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(JobDataAccessor.Delete(UniqueID)));
        }

        public ActionResult InitDetailTree(string UniqueID)
        {
            try
            {
                RequestResult result = JobDataAccessor.GetDetailTreeItem(UniqueID);

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

        public ActionResult GetDetailTreeItem(string JobUniqueID, string ControlPointUniqueID, string EquipmentUniqueID, string PartUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = JobDataAccessor.GetDetailTreeItem(JobUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID);

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

                    result = JobDataAccessor.AddUser(model.UserList, selectedList, Session["Account"] as Account);

                    if (result.IsSuccess)
                    {
                        model.UserList = result.Data as List<Models.EquipmentMaintenance.JobManagement.UserModel>;

                        Session["JobCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["JobEditFormModel"] as EditFormModel;

                    result = JobDataAccessor.AddUser(model.UserList, selectedList, Session["Account"] as Account);

                    if (result.IsSuccess)
                    {
                        model.UserList = result.Data as List<Models.EquipmentMaintenance.JobManagement.UserModel>;

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
    }
}