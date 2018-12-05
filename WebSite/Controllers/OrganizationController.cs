using System;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.OrganizationManagement;
using System.Linq;

#if ASE
using DataAccess.ASE;
#else
using DataAccess;
#endif

using System.Web;

namespace WebSite.Controllers
{
    public class OrganizationController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Detail(string UniqueID)
        {
            try
            {
                RequestResult result = OrganizationDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_Detail", result.Data);
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
        public ActionResult Create(string ParentOrganizationUniqueID)
        {
            try
            {
                if (ParentOrganizationUniqueID == "*")
                {
                    Session["OrganizationCreateFormModel"] = new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID = "*",
                        ParentUniqueID = "*",
                        ParentOrganizationFullDescription = "*"
                    };
                }
                else
                {
                    var parentOrganization = OrganizationDataAccessor.GetOrganization(ParentOrganizationUniqueID);

                    Session["OrganizationCreateFormModel"] = new CreateFormModel()
                    {
                        AncestorOrganizationUniqueID = parentOrganization.AncestorOrganizationUniqueID,
                        ParentUniqueID = parentOrganization.UniqueID,
                        ParentOrganizationFullDescription = parentOrganization.FullDescription
                    };
                }

                Session["OrganizationFormAction"] = Define.EnumFormAction.Create;

                return PartialView("_Create", Session["OrganizationCreateFormModel"]);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["OrganizationCreateFormModel"] as CreateFormModel;

                model.FormInput = Model.FormInput;

                result = OrganizationDataAccessor.Create(model);

                if (result.IsSuccess)
                {
                    HttpRuntime.Cache.Remove("Organizations");

                    Session.Remove("OrganizationFormAction");
                    Session.Remove("OrganizationCreateFormModel");

                    var account = Session["Account"] as Account;

                    account.UserOrganizationPermissionList = OrganizationDataAccessor.GetUserOrganizationPermissionList(account.OrganizationUniqueID);

                    Session["Account"] = account;
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
            try
            {
                RequestResult result = OrganizationDataAccessor.GetEditFormModel(UniqueID);

                if (result.IsSuccess)
                {
                    Session["OrganizationFormAction"] = Define.EnumFormAction.Edit;
                    Session["OrganizationEditFormModel"] = result.Data;

                    return PartialView("_Edit", Session["OrganizationEditFormModel"]);
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

        [HttpPost]
        public ActionResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["OrganizationEditFormModel"] as EditFormModel;

                model.FormInput = Model.FormInput;

                result = OrganizationDataAccessor.Edit(model);

                if (result.IsSuccess)
                {
                    HttpRuntime.Cache.Remove("Organizations");

                    Session.Remove("OrganizationFormAction");
                    Session.Remove("OrganizationEditFormModel");

                    var account = Session["Account"] as Account;

                    account.UserOrganizationPermissionList = OrganizationDataAccessor.GetUserOrganizationPermissionList(account.OrganizationUniqueID);

                    Session["Account"] = account;
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
            RequestResult result = OrganizationDataAccessor.Delete(UniqueID);

            if (result.IsSuccess)
            {
                HttpRuntime.Cache.Remove("Organizations");
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult InitTree()
        {
            try
            {
                var account = Session["Account"] as Account;

                account.UserOrganizationPermissionList = OrganizationDataAccessor.GetUserOrganizationPermissionList(account.OrganizationUniqueID);

                Session["Account"] = account;

                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = new RequestResult();

                if (account.UserRootOrganizationUniqueID == "*")
                {
                    result = OrganizationDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, account);
                }
                else
                {
                    result = OrganizationDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, account);
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

                RequestResult result = OrganizationDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, Session["Account"] as Account);

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

        public ActionResult InitManagerTree(string AncestorOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = UserDataAccessor.GetRootTreeItem(organizationList, AncestorOrganizationUniqueID, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_ManagerTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetManagerTreeItem(string OrganizationUniqueID)
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

        public ActionResult InitEditableOrganizationSelectTree(string EditableAncestorOrganizationUniqueID, string AncestorOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = OrganizationDataAccessor.GetEditableOrganizationRootTreeItem(organizationList, EditableAncestorOrganizationUniqueID, AncestorOrganizationUniqueID, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_EditableOrganizationSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetEditableOrganizationSelectTreeItem(string EditableAncestorOrganizationUniqueID, string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = OrganizationDataAccessor.GetEditableOrganizationTreeItem(organizationList, EditableAncestorOrganizationUniqueID, OrganizationUniqueID, Session["Account"] as Account);

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

        public ActionResult GetEditableOrganizationSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["OrganizationFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_EditableOrganizationSelectedList", (Session["OrganizationCreateFormModel"] as CreateFormModel).EditableOrganizationList);
                }
                else if ((Define.EnumFormAction)Session["OrganizationFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_EditableOrganizationSelectedList", (Session["OrganizationEditFormModel"] as EditFormModel).EditableOrganizationList);
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

        public ActionResult AddEditableOrganization(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                if ((Define.EnumFormAction)Session["OrganizationFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["OrganizationCreateFormModel"] as CreateFormModel;

                    result = OrganizationDataAccessor.AddEditableOrganization(model.EditableOrganizationList, selectedList, Session["Account"] as Account);

                    if (result.IsSuccess)
                    {
                        model.EditableOrganizationList = result.Data as List<EditableOrganizationModel>;

                        Session["OrganizationCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["OrganizationFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["OrganizationEditFormModel"] as EditFormModel;

                    result = OrganizationDataAccessor.AddEditableOrganization(model.EditableOrganizationList, selectedList, Session["Account"] as Account);

                    if (result.IsSuccess)
                    {
                        model.EditableOrganizationList = result.Data as List<EditableOrganizationModel>;

                        Session["OrganizationEditFormModel"] = model;
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

        public ActionResult DeleteEditableOrganization(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                if ((Define.EnumFormAction)Session["OrganizationFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["OrganizationCreateFormModel"] as CreateFormModel;

                    model.EditableOrganizationList.Remove(model.EditableOrganizationList.First(x => x.UniqueID == UniqueID));

                    Session["OrganizationCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["OrganizationFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["OrganizationEditFormModel"] as EditFormModel;

                    model.EditableOrganizationList.Remove(model.EditableOrganizationList.First(x => x.UniqueID == UniqueID));

                    Session["OrganizationEditFormModel"] = model;

                    result.Success();
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

        public ActionResult InitQueryableOrganizationSelectTree(string EditableAncestorOrganizationUniqueID, string AncestorOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = OrganizationDataAccessor.GetQueryableOrganizationRootTreeItem(organizationList, EditableAncestorOrganizationUniqueID, AncestorOrganizationUniqueID, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_QueryableOrganizationSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetQueryableOrganizationSelectTreeItem(string EditableAncestorOrganizationUniqueID, string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = OrganizationDataAccessor.GetQueryableOrganizationTreeItem(organizationList, EditableAncestorOrganizationUniqueID, OrganizationUniqueID, Session["Account"] as Account);

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

        public ActionResult GetQueryableOrganizationSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["OrganizationFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_QueryableOrganizationSelectedList", (Session["OrganizationCreateFormModel"] as CreateFormModel).QueryableOrganizationList);
                }
                else if ((Define.EnumFormAction)Session["OrganizationFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_QueryableOrganizationSelectedList", (Session["OrganizationEditFormModel"] as EditFormModel).QueryableOrganizationList);
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

        public ActionResult AddQueryableOrganization(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                if ((Define.EnumFormAction)Session["OrganizationFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["OrganizationCreateFormModel"] as CreateFormModel;

                    result = OrganizationDataAccessor.AddQueryableOrganization(model.QueryableOrganizationList, selectedList, Session["Account"] as Account);

                    if (result.IsSuccess)
                    {
                        model.QueryableOrganizationList = result.Data as List<QueryableOrganizationModel>;

                        Session["OrganizationCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["OrganizationFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["OrganizationEditFormModel"] as EditFormModel;

                    result = OrganizationDataAccessor.AddQueryableOrganization(model.QueryableOrganizationList, selectedList, Session["Account"] as Account);

                    if (result.IsSuccess)
                    {
                        model.QueryableOrganizationList = result.Data as List<QueryableOrganizationModel>;

                        Session["OrganizationEditFormModel"] = model;
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

        public ActionResult DeleteQueryableOrganization(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                if ((Define.EnumFormAction)Session["OrganizationFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["OrganizationCreateFormModel"] as CreateFormModel;

                    model.QueryableOrganizationList.Remove(model.QueryableOrganizationList.First(x => x.UniqueID == UniqueID));

                    Session["OrganizationCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["OrganizationFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["OrganizationEditFormModel"] as EditFormModel;

                    model.QueryableOrganizationList.Remove(model.QueryableOrganizationList.First(x => x.UniqueID == UniqueID));

                    Session["OrganizationEditFormModel"] = model;

                    result.Success();
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