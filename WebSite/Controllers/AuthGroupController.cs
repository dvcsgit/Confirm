using System;
using System.Reflection;
using System.ComponentModel;
using System.Web.Mvc;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.AuthGroupManagement;
using System.Linq;
using System.Web;
#if ASE
using DataAccess.ASE;
using Models.Authenticated;
#else
using DataAccess;
using Models.Authenticated;
#endif

namespace WebSite.Controllers
{
    public class AuthGroupController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = AuthGroupDataAccessor.Query(Parameters);

            if (result.IsSuccess)
            {
                return PartialView("_List", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Detail(string AuthGroupID)
        {
            RequestResult result = AuthGroupDataAccessor.GetDetailViewModel(AuthGroupID);

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
        public ActionResult Create()
        {
            RequestResult result = AuthGroupDataAccessor.GetCreateFormModel(Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["AuthGroupFormAction"] = Define.EnumFormAction.Create;
                Session["AuthGroupCreateFormModel"] = result.Data;

                return PartialView("_Create", Session["AuthGroupCreateFormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Copy(string AuthGroupID)
        {
            RequestResult result = AuthGroupDataAccessor.GetCopyFormModel(AuthGroupID, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["AuthGroupFormAction"] = Define.EnumFormAction.Create;
                Session["AuthGroupCreateFormModel"] = result.Data;

                return PartialView("_Create", Session["AuthGroupCreateFormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["AuthGroupCreateFormModel"] as CreateFormModel;

                model.FormInput = Model.FormInput;

                result = AuthGroupDataAccessor.Create(model);
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
        public ActionResult Edit(string AuthGroupID)
        {
            RequestResult result = AuthGroupDataAccessor.GetEditFormModel(AuthGroupID, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["AuthGroupFormAction"] = Define.EnumFormAction.Edit;
                Session["AuthGroupEditFormModel"] = result.Data;

                return PartialView("_Edit", Session["AuthGroupEditFormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["AuthGroupEditFormModel"] as EditFormModel;

                model.FormInput = Model.FormInput;

                result = AuthGroupDataAccessor.Edit(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult Delete(string AuthGroupID)
        {
            return Content(JsonConvert.SerializeObject(AuthGroupDataAccessor.Delete(AuthGroupID)));
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
                if ((Define.EnumFormAction)Session["AuthGroupFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedUserList", (Session["AuthGroupCreateFormModel"] as CreateFormModel).UserList);
                }
                else if ((Define.EnumFormAction)Session["AuthGroupFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedUserList", (Session["AuthGroupEditFormModel"] as EditFormModel).UserList);
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

                if ((Define.EnumFormAction)Session["AuthGroupFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["AuthGroupCreateFormModel"] as CreateFormModel;

                    result = AuthGroupDataAccessor.AddUser(model.UserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.UserList = result.Data as List<Models.AuthGroupManagement.UserModel>;

                        Session["AuthGroupCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["AuthGroupFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["AuthGroupEditFormModel"] as EditFormModel;

                    result = AuthGroupDataAccessor.AddUser(model.UserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.UserList = result.Data as List<Models.AuthGroupManagement.UserModel>;

                        Session["AuthGroupEditFormModel"] = model;
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
                if ((Define.EnumFormAction)Session["AuthGroupFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["AuthGroupCreateFormModel"] as CreateFormModel;

                    model.UserList.Remove(model.UserList.First(x => x.ID == UserID));

                    Session["AuthGroupCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["AuthGroupFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["AuthGroupEditFormModel"] as EditFormModel;

                    model.UserList.Remove(model.UserList.First(x => x.ID == UserID));

                    Session["AuthGroupEditFormModel"] = model;

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