using System;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.RepairFormManagement;
using System.Linq;
using System.Web.Security;
using System.Web;
using Webdiyer.WebControls.Mvc;

#if ASE
using DataAccess.ASE;
using System.ComponentModel;
#else
using DataAccess;
using DataAccess.EquipmentMaintenance;
using System.ComponentModel;
#endif

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class RepairFormController : Controller
    {
        public ActionResult Index(string RepairFormUniqueID, string CheckResultUniqueID, string Status, string VHNO)
        {
            RequestResult result = RepairFormDataAccessor.GetQueryFormModel(RepairFormUniqueID, CheckResultUniqueID, Status, VHNO, Session["Account"] as Account);

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
            RequestResult result = RepairFormDataAccessor.Query(Model.Parameters, Session["Account"] as Account);

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
            RequestResult result = RepairFormDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["RepairFormDetailViewModel"] = result.Data;

                return PartialView("_Detail", Session["RepairFormDetailViewModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Job()
        {
            return Content(JsonConvert.SerializeObject(RepairFormDataAccessor.Job(Session["RepairFormDetailViewModel"] as DetailViewModel)));
        }

        public ActionResult Refuse(string UniqueID, string RefuseReason)
        {
            return Content(JsonConvert.SerializeObject(RepairFormDataAccessor.Refuse(UniqueID, RefuseReason)));
        }

        public ActionResult TakeJob(string UniqueID, string EstBeginDateString, string EstEndDateString, string RealTakeJobDateString,string RealTakeJobHour, string RealTakeJobMin, string RealTakeJobUserID)
        {
            return Content(JsonConvert.SerializeObject(RepairFormDataAccessor.TakeJob(UniqueID, EstBeginDateString, EstEndDateString,RealTakeJobDateString,RealTakeJobHour,RealTakeJobMin,RealTakeJobUserID, Session["Account"] as Account)));
        }

        public ActionResult RefuseJob(string UniqueID, string JobRefuseReason)
        {
            return Content(JsonConvert.SerializeObject(RepairFormDataAccessor.RefuseJob(UniqueID, JobRefuseReason, Session["Account"] as Account)));
        }

        [HttpGet]
        public ActionResult Create(string OrganizationUniqueID, string CheckResultUniqueID)
        {
            RequestResult result = RepairFormDataAccessor.GetCreateFormModel(OrganizationUniqueID, CheckResultUniqueID);

            if (result.IsSuccess)
            {
                Session["FormAction"] = Define.EnumFormAction.Create;

                Session["RepairFormCreateFormModel"] = result.Data;

                return PartialView("_Create", Session["RepairFormCreateFormModel"]);
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
                var model = Session["RepairFormCreateFormModel"] as CreateFormModel;

                Model.FileList = model.FileList;

                result = RepairFormDataAccessor.Create(Model, (Session["Account"] as Account).ID);
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
            RequestResult result = RepairFormDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["FormAction"] = Define.EnumFormAction.Edit;

                Session["RepairFormEditFormModel"] = result.Data;

                return PartialView("_Edit", Session["RepairFormEditFormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model, string ColumnPageStates, string MaterialPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["RepairFormEditFormModel"] as EditFormModel;

                var columnPageStateList = JsonConvert.DeserializeObject<List<string>>(ColumnPageStates);

                result = RepairFormDataAccessor.SavePageState(model.ColumnList, columnPageStateList);

                if (result.IsSuccess)
                {
                    model.ColumnList = result.Data as List<ColumnModel>;

                    var materialPageStateList = JsonConvert.DeserializeObject<List<string>>(MaterialPageStates);

                    result = RepairFormDataAccessor.SavePageState(model.MaterialList, materialPageStateList);

                    if (result.IsSuccess)
                    {
                        model.MaterialList = result.Data as List<MaterialModel>;

                        result = RepairFormDataAccessor.Edit(model);
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

        [HttpPost]
        public ActionResult Submit(EditFormModel Model, string ColumnPageStates, string MaterialPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["RepairFormEditFormModel"] as EditFormModel;

                var columnPageStateList = JsonConvert.DeserializeObject<List<string>>(ColumnPageStates);

                result = RepairFormDataAccessor.SavePageState(model.ColumnList, columnPageStateList);

                if (result.IsSuccess)
                {
                    model.ColumnList = result.Data as List<ColumnModel>;

                    var materialPageStateList = JsonConvert.DeserializeObject<List<string>>(MaterialPageStates);

                    result = RepairFormDataAccessor.SavePageState(model.MaterialList, materialPageStateList);

                    if (result.IsSuccess)
                    {
                        model.MaterialList = result.Data as List<MaterialModel>;

                        result = RepairFormDataAccessor.Edit(model);

                        if (result.IsSuccess)
                        {
                            result = RepairFormDataAccessor.Submit(model.UniqueID, Session["Account"] as Account);
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

        public ActionResult Approve(string UniqueID, string Remark)
        {
            return Content(JsonConvert.SerializeObject(RepairFormDataAccessor.Approve(UniqueID, Remark, Session["Account"] as Account)));
        }

        public ActionResult Reject(string UniqueID, string Remark)
        {
            return Content(JsonConvert.SerializeObject(RepairFormDataAccessor.Reject(UniqueID, Remark, Session["Account"] as Account)));
        }

        public ActionResult GetWorkingHourList()
        {
            try
            {
                return PartialView("_WorkingHourList", (Session["RepairFormEditFormModel"] as EditFormModel).WorkingHourList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        [HttpGet]
        public ActionResult CreateWorkingHour()
        {
            return PartialView("_CreateWorkingHour", new CreateWorkingHourFormModel());
        }

        [HttpPost]
        public ActionResult CreateWorkingHour(CreateWorkingHourFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["RepairFormEditFormModel"] as EditFormModel;

                result = RepairFormDataAccessor.CreateWorkingHour(model.WorkingHourList, Model);

                if (result.IsSuccess)
                {
                    model.WorkingHourList = result.Data as List<WorkingHourModel>;

                    Session["RepairFormEditFormModel"] = model;
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

        public ActionResult DeleteWorkingHour(int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["RepairFormEditFormModel"] as EditFormModel;

                model.WorkingHourList.Remove(model.WorkingHourList.First(x => x.Seq == Seq));

                Session["RepairFormEditFormModel"] = model;

                result.Success();
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

        public ActionResult InitUserTree(string AncestorOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = UserDataAccessor.GetRootTreeItem(organizationList, AncestorOrganizationUniqueID, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_UserTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetUserTreeItem(string OrganizationUniqueID)
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
                return PartialView("_SelectedUserList", (Session["RepairFormDetailViewModel"] as DetailViewModel).JobUserList);
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

                var model = Session["RepairFormDetailViewModel"] as DetailViewModel;

                result = RepairFormDataAccessor.AddUser(model.JobUserList, selectedList);

                if (result.IsSuccess)
                {
                    model.JobUserList = result.Data as List<UserModel>;

                    Session["RepairFormDetailViewModel"] = model;
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
                var model = Session["RepairFormDetailViewModel"] as DetailViewModel;

                model.JobUserList.Remove(model.JobUserList.First(x => x.ID == UserID));

                Session["RepairFormDetailViewModel"] = model;

                result.Success();
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodInfo.GetCurrentMethod(), ex);

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
                    result = OrganizationDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
                }
                else
                {
                    result = OrganizationDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
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
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                jsonTree = string.Empty;
            }

            return Content(jsonTree);
        }

        public ActionResult InitMaterialSelectTree(string RefOrganizationUniqueID, string EquipmentUniqueID, string PartUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = MaterialDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, EquipmentUniqueID, PartUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_MaterialSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetMaterialSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string EquipmentType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = MaterialDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, EquipmentType);

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

        public ActionResult GetMaterialSelectedList()
        {
            try
            {
                return PartialView("_MaterialSelectedList", (Session["RepairFormEditFormModel"] as EditFormModel).MaterialList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult AddMaterial(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                var model = Session["RepairFormEditFormModel"] as EditFormModel;

                result = RepairFormDataAccessor.SavePageState(model.MaterialList, pageStateList);

                if (result.IsSuccess)
                {
                    model.MaterialList = result.Data as List<MaterialModel>;

                    result = RepairFormDataAccessor.AddMaterial(model.MaterialList, selectedList, RefOrganizationUniqueID);

                    if (result.IsSuccess)
                    {
                        model.MaterialList = result.Data as List<MaterialModel>;

                        Session["RepairFormEditFormModel"] = model;
                    }
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

        public ActionResult DeleteMaterial(int Seq, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                var model = Session["RepairFormEditFormModel"] as EditFormModel;

                result = RepairFormDataAccessor.SavePageState(model.MaterialList, pageStateList);

                if (result.IsSuccess)
                {
                    model.MaterialList = result.Data as List<MaterialModel>;

                    model.MaterialList.Remove(model.MaterialList.First(x => x.Seq == Seq));

                    Session["RepairFormEditFormModel"] = model;

                    result.Success();
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

        public ActionResult Export()
        {
            var model = RepairFormDataAccessor.Export(Session["RepairFormDetailViewModel"] as DetailViewModel);

            return File(model.Data, model.ContentType, model.FileName);
        }

        public ActionResult GetFileList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_FileList", (Session["RepairFormCreateFormModel"] as CreateFormModel).FileList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_FileList", (Session["RepairFormEditFormModel"] as EditFormModel).FileList);
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
                        var model = Session["RepairFormCreateFormModel"] as CreateFormModel;

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
                            UploadTime = DateTime.Now,
                            Size = Request.Files[0].ContentLength,
                            IsSaved = false
                        };

                        Request.Files[0].SaveAs(fileModel.TempFullFileName);

                        model.FileList.Add(fileModel);

                        result.Success();
                    }
                    else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                    {
                        var model = Session["RepairFormEditFormModel"] as EditFormModel;

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
                            UploadTime = DateTime.Now,
                            Size = Request.Files[0].ContentLength,
                            IsSaved = false
                        };

                        Request.Files[0].SaveAs(fileModel.TempFullFileName);

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

        public ActionResult GetUserOptions([DefaultValue(1)]int PageIndex, [DefaultValue(10)]int PageSize, string Term, [DefaultValue(false)]bool IsInit)
        {
            var accountList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = RepairFormDataAccessor.GetUserOptions(accountList, Term, IsInit);

            if (result.IsSuccess)
            {
                var queryResult = result.Data as List<SelectListItem>;

                var data = queryResult.Select(x => new { id = x.Value, text = x.Text, name = x.Text }).AsQueryable().OrderBy(x => x.id).ToPagedList(PageIndex, PageSize);

                return Json(new { Success = true, Data = data, Total = data.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, Message = result.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteFile(int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RepairFormCreateFormModel"] as CreateFormModel;

                    model.FileList.Remove(model.FileList.First(x => x.Seq == Seq));

                    Session["RepairFormCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RepairFormEditFormModel"] as EditFormModel;

                    model.FileList.Remove(model.FileList.First(x => x.Seq == Seq));

                    Session["RepairFormEditFormModel"] = model;

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

        public ActionResult Download(string RFormUniqueID, int Seq)
        {
            var file = RepairFormDataAccessor.GetFile(RFormUniqueID, Seq);

            return File(file.FullFileName, file.ContentType, file.Display);
        }
    }
}
