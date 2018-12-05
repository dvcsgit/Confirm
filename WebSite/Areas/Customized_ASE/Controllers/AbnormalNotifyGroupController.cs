#if ASE
using DataAccess.ASE;
using Models.ASE.AbnormalNotifyGroup;
using Models.Authenticated;
using Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;
using Webdiyer.WebControls.Mvc;

namespace WebSite.Areas.Customized_ASE.Controllers
{
    public class AbnormalNotifyGroupController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = AbnormalNotifyGroupDataAccessor.Query(Parameters);

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
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = AbnormalNotifyGroupDataAccessor.GetDetailViewModel(UniqueID, userList);

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
            Session["FormAction"] = Define.EnumFormAction.Create;
            Session["FormModel"] = new CreateFormModel();

            return PartialView("_Create", Session["FormModel"]);
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["FormModel"] as CreateFormModel;

                model.FormInput = Model.FormInput;

                result = AbnormalNotifyGroupDataAccessor.Create(model);

                if (result.IsSuccess)
                {
                    Session.Remove("FormAction");
                    Session.Remove("FormModel");
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
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = AbnormalNotifyGroupDataAccessor.GetEditFormModel(UniqueID, Session["Account"] as Account, userList);

            if (result.IsSuccess)
            {
                Session["FormAction"] = Define.EnumFormAction.Edit;
                Session["FormModel"] = result.Data;

                return PartialView("_Edit", Session["FormModel"]);
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
                var model = Session["FormModel"] as EditFormModel;

                model.FormInput = Model.FormInput;

                result = AbnormalNotifyGroupDataAccessor.Edit(model);

                if (result.IsSuccess)
                {
                    Session.Remove("FormAction");
                    Session.Remove("FormModel");
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
            return Content(JsonConvert.SerializeObject(AbnormalNotifyGroupDataAccessor.Delete(UniqueID)));
        }

        public ActionResult InitSelectUserTree()
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                RequestResult result = AbnormalNotifyGroupDataAccessor.GetUserTreeItem(organizationList, userList, "2a54f076-f14c-44fd-9f42-b202ac9206e0");

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
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                RequestResult result = AbnormalNotifyGroupDataAccessor.GetUserTreeItem(organizationList, userList, OrganizationUniqueID);

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
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedUserList", (Session["FormModel"] as CreateFormModel).UserList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedUserList", (Session["FormModel"] as EditFormModel).UserList);
                }
                else
                {
                    return PartialView("_Error", new Error(MethodBase.GetCurrentMethod(), Resources.Resource.UnKnownOperation));
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

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = AbnormalNotifyGroupDataAccessor.AddUser(model.UserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.UserList = result.Data as List<Models.ASE.Shared.ASEUserModel>;

                        Session["FormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = AbnormalNotifyGroupDataAccessor.AddUser(model.UserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.UserList = result.Data as List<Models.ASE.Shared.ASEUserModel>;

                        Session["FormModel"] = model;
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
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    model.UserList.Remove(model.UserList.First(x => x.ID == UserID));

                    Session["FormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    model.UserList.Remove(model.UserList.First(x => x.ID == UserID));

                    Session["FormModel"] = model;

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



        public ActionResult InitSelectCCUserTree()
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                RequestResult result = AbnormalNotifyGroupDataAccessor.GetUserTreeItem(organizationList, userList, "2a54f076-f14c-44fd-9f42-b202ac9206e0");

                if (result.IsSuccess)
                {
                    return PartialView("_SelectCCUserTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSelectCCUserTreeItem(string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                RequestResult result = AbnormalNotifyGroupDataAccessor.GetUserTreeItem(organizationList, userList, OrganizationUniqueID);

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

        public ActionResult GetSelectedCCUserList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedCCUserList", (Session["FormModel"] as CreateFormModel).CCUserList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedCCUserList", (Session["FormModel"] as EditFormModel).CCUserList);
                }
                else
                {
                    return PartialView("_Error", new Error(MethodBase.GetCurrentMethod(), Resources.Resource.UnKnownOperation));
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodInfo.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult AddCCUser(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    result = AbnormalNotifyGroupDataAccessor.AddUser(model.CCUserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.CCUserList = result.Data as List<Models.ASE.Shared.ASEUserModel>;

                        Session["FormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = AbnormalNotifyGroupDataAccessor.AddUser(model.CCUserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.CCUserList = result.Data as List<Models.ASE.Shared.ASEUserModel>;

                        Session["FormModel"] = model;
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

        public ActionResult DeleteCCUser(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    model.CCUserList.Remove(model.CCUserList.First(x => x.ID == UserID));

                    Session["FormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    model.CCUserList.Remove(model.CCUserList.First(x => x.ID == UserID));

                    Session["FormModel"] = model;

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
#endif