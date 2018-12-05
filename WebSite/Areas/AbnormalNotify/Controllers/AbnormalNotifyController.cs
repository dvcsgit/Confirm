using DataAccess;
using DataAccess.AbnormalNotify;
using Models.AbnormalNotify.AbnormalNotify;
using Models.Authenticated;
using Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.AbnormalNotify.Controllers
{
    public class AbnormalNotifyController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = AbnormalNotifyDataAccessor.Query(Model.Parameters, organizationList, userList);

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
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = AbnormalNotifyDataAccessor.GetDetailViewModel(UniqueID, organizationList, userList);

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
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = AbnormalNotifyDataAccessor.GetCreateFormModel(organizationList, userList);

            if (result.IsSuccess)
            {
                Session["FormAction"] = Define.EnumFormAction.Create;
                Session["FormModel"] = result.Data;

                return PartialView("_Create", Session["FormModel"]);
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
                var model = Session["FormModel"] as CreateFormModel;

                model.FormInput = Model.FormInput;

                result = AbnormalNotifyDataAccessor.Create(model, Session["Account"] as Account);

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
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = AbnormalNotifyDataAccessor.GetEditFormModel(UniqueID,  userList);

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

                result = AbnormalNotifyDataAccessor.Edit(model, Session["Account"] as Account);

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

        public ActionResult Download(string FormUniqueID, int Seq)
        {
            var file = AbnormalNotifyDataAccessor.GetFile(FormUniqueID, Seq);

            return File(file.FullFileName, file.ContentType, file.Display);
        }

        public ActionResult GetFileList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_FileList", (Session["FormModel"] as CreateFormModel).FileList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_FileList", (Session["FormModel"] as EditFormModel).FileList);
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

        [HttpGet]
        public ActionResult FileUpload()
        {
            return PartialView("_FileUpload");
        }

        [HttpPost]
        public ActionResult Upload()
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                    {
                        var model = Session["FormModel"] as CreateFormModel;

                        int seq = 1;

                        if (model.FileList.Count > 0)
                        {
                            seq = model.FileList.Max(x => x.Seq) + 1;
                        }

                        var fileName = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('\\') + 1);

                        var fileModel = new FileModel()
                        {
                            TempUniqueID = Guid.NewGuid().ToString(),
                            Seq = seq,
                            FileName = fileName.Substring(0, fileName.LastIndexOf('.')),
                            Extension = fileName.Substring(fileName.LastIndexOf('.') + 1),
                            LastModifyTime = DateTime.Now,
                            Size = Request.Files[0].ContentLength,
                            IsSaved = false
                        };

                        Request.Files[0].SaveAs(Path.Combine(Config.TempFolder, fileModel.TempFileName));

                        model.FileList.Add(fileModel);

                        result.Success();
                    }
                    else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                    {
                        var model = Session["FormModel"] as EditFormModel;

                        int seq = 1;

                        if (model.FileList.Count > 0)
                        {
                            seq = model.FileList.Max(x => x.Seq) + 1;
                        }

                        var fileName = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('\\') + 1);

                        var fileModel = new FileModel()
                        {
                            TempUniqueID = Guid.NewGuid().ToString(),
                            Seq = seq,
                            FileName = fileName.Substring(0, fileName.LastIndexOf('.')),
                            Extension = fileName.Substring(fileName.LastIndexOf('.') + 1),
                            LastModifyTime = DateTime.Now,
                            Size = Request.Files[0].ContentLength,
                            IsSaved = false
                        };

                        Request.Files[0].SaveAs(Path.Combine(Config.TempFolder, fileModel.TempFileName));

                        model.FileList.Add(fileModel);

                        result.Success();
                    }
                    else
                    {
                        result.ReturnFailedMessage(Resources.Resource.UnKnownOperation);
                    }
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UploadFileRequired);
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

        public ActionResult DeleteFile(int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    model.FileList.Remove(model.FileList.First(x => x.Seq == Seq));

                    Session["FormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    model.FileList.Remove(model.FileList.First(x => x.Seq == Seq));

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
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }



        public ActionResult InitSelectUserTree()
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                RequestResult result = AbnormalNotifyDataAccessor.GetUserTreeItem(organizationList, userList, "*");

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

                RequestResult result = AbnormalNotifyDataAccessor.GetUserTreeItem(organizationList, userList, OrganizationUniqueID);

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
                    return PartialView("_SelectedUserList", (Session["FormModel"] as CreateFormModel).NotifyUserList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedUserList", (Session["FormModel"] as EditFormModel).NotifyUserList);
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

                    result = AbnormalNotifyDataAccessor.AddUser(model.NotifyUserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.NotifyUserList = result.Data as List<UserModel>;

                        Session["FormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = AbnormalNotifyDataAccessor.AddUser(model.NotifyUserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.NotifyUserList = result.Data as List<UserModel>;

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

                    model.NotifyUserList.Remove(model.NotifyUserList.First(x => x.ID == UserID));

                    Session["FormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    model.NotifyUserList.Remove(model.NotifyUserList.First(x => x.ID == UserID));

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

                RequestResult result = AbnormalNotifyDataAccessor.GetUserTreeItem(organizationList, userList, "*");

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

                RequestResult result = AbnormalNotifyDataAccessor.GetUserTreeItem(organizationList, userList, OrganizationUniqueID);

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
                    return PartialView("_SelectedCCUserList", (Session["FormModel"] as CreateFormModel).NotifyCCUserList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedCCUserList", (Session["FormModel"] as EditFormModel).NotifyCCUserList);
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

                    result = AbnormalNotifyDataAccessor.AddUser(model.NotifyCCUserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.NotifyCCUserList = result.Data as List<UserModel>;

                        Session["FormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = AbnormalNotifyDataAccessor.AddUser(model.NotifyCCUserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.NotifyCCUserList = result.Data as List<UserModel>;

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

                    model.NotifyCCUserList.Remove(model.NotifyCCUserList.First(x => x.ID == UserID));

                    Session["FormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    model.NotifyCCUserList.Remove(model.NotifyCCUserList.First(x => x.ID == UserID));

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