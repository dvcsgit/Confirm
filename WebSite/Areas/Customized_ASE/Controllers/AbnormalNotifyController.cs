#if ASE
using DataAccess.ASE;
using Models.ASE.AbnormalNotify;
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
using System.IO;

namespace WebSite.Areas.Customized_ASE.Controllers
{
    public class AbnormalNotifyController : Controller
    {
        public ActionResult Index(string Status)
        {
            return View(new QueryFormModel()
            {
                Parameters = new QueryParameters()
                {
                    Status = Status
                }
            });
        }

        public ActionResult Portal(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = AbnormalNotifyDataAccessor.GetDetailViewModel(UniqueID, organizationList, userList);

            if (result.IsSuccess)
            {
                return View(result.Data);
            }
            else
            {
                ViewBag.Error = result.Error;

                return View("Error");
            }
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

            RequestResult result = AbnormalNotifyDataAccessor.GetCreateFormModel(organizationList.Where(x => x.ParentUniqueID == "1c8932c1-9cdd-4834-8b1b-d975cf3a0cc2").OrderBy(x => x.ID).ToList(),organizationList, userList);

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

            RequestResult result = AbnormalNotifyDataAccessor.GetEditFormModel(UniqueID, organizationList.Where(x => x.ParentUniqueID == "1c8932c1-9cdd-4834-8b1b-d975cf3a0cc2").OrderBy(x => x.ID).ToList(), userList);

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

        [HttpGet]
        public ActionResult TakeJob(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = AbnormalNotifyDataAccessor.GetDetailViewModel(UniqueID, organizationList, userList);

            if (result.IsSuccess)
            {
                return PartialView("_TakeJob", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult TakeJob(DetailViewModel Model)
        {
            return Content(JsonConvert.SerializeObject(AbnormalNotifyDataAccessor.TakeJob(Model.UniqueID, Session["Account"] as Account)));
        }

        [HttpGet]
        public ActionResult Closed(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = AbnormalNotifyDataAccessor.GetClosedFormModel(UniqueID, organizationList, userList);

            if (result.IsSuccess)
            {
                return PartialView("_Closed", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Closed(ClosedFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(AbnormalNotifyDataAccessor.Closed(Model)));
        }

        [HttpGet]
        public ActionResult RepairForm(string UniqueID)
        {
            RequestResult result = AbnormalNotifyDataAccessor.GetCreateRepairFormModel(UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_RepairForm", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult RepairForm(RepairFormCreateFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(AbnormalNotifyDataAccessor.CreateRepairForm(Model, (Session["Account"] as Account).ID)));
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

                RequestResult result = AbnormalNotifyDataAccessor.GetUserTreeItem(organizationList, userList, "2a54f076-f14c-44fd-9f42-b202ac9206e0");

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
                        model.NotifyUserList = result.Data as List<Models.ASE.Shared.ASEUserModel>;

                        Session["FormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = AbnormalNotifyDataAccessor.AddUser(model.NotifyUserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.NotifyUserList = result.Data as List<Models.ASE.Shared.ASEUserModel>;

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

        public ActionResult InitMaintenanceOrganizationTree(string OrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = OrganizationDataAccessor.GetRootTreeItem(organizationList, OrganizationUniqueID, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_MaintenanceOrganizationTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetMaintenanceOrganizationTreeItem(string OrganizationUniqueID)
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


        public ActionResult InitSelectCCUserTree()
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                RequestResult result = AbnormalNotifyDataAccessor.GetUserTreeItem(organizationList, userList, "2a54f076-f14c-44fd-9f42-b202ac9206e0");

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
                        model.NotifyCCUserList = result.Data as List<Models.ASE.Shared.ASEUserModel>;

                        Session["FormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    result = AbnormalNotifyDataAccessor.AddUser(model.NotifyCCUserList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.NotifyCCUserList = result.Data as List<Models.ASE.Shared.ASEUserModel>;

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
#endif