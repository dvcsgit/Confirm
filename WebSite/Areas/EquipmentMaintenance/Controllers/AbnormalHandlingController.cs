using Models.Authenticated;
using Models.EquipmentMaintenance.AbnormalHandlingManagement;
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

#if ASE
using DataAccess.ASE;
#else
using DataAccess;
using DataAccess.EquipmentMaintenance;
#endif

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class AbnormalHandlingController : Controller
    {
        public ActionResult Index(string Status, string UniqueID)
        {
            return View(new QueryFormModel()
            {
                Parameters = new QueryParameters()
                {
                    BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
                    EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
                    Status = Status,
                    UniqueID = UniqueID
                }
            });
        }

        public ActionResult Query(QueryFormModel Model)
        {
            RequestResult result = AbnormalHandlingDataAccessor.Query(Model.Parameters, Session["Account"] as Account);

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
            RequestResult result = AbnormalHandlingDataAccessor.GetDetailViewModel(UniqueID);

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
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = AbnormalHandlingDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["EditFormModel"] = result.Data;

                return PartialView("_Edit", Session["EditFormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Save(string Remark)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EditFormModel"] as EditFormModel;

                model.ClosedRemark = Remark;

                result = AbnormalHandlingDataAccessor.Save(model);

                if (result.IsSuccess)
                {
                    Session.Remove("EditFormModel");
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
        public ActionResult Closed(string Remark)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EditFormModel"] as EditFormModel;

                model.ClosedRemark = Remark;

                result = AbnormalHandlingDataAccessor.Closed(model, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    Session.Remove("EditFormModel");
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

        public ActionResult GetBeforePhotoList()
        {
            try
            {
                return PartialView("_BeforePhotoList", (Session["EditFormModel"] as EditFormModel).BeforePhotoList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult GetAfterPhotoList()
        {
            try
            {
                return PartialView("_AfterPhotoList", (Session["EditFormModel"] as EditFormModel).AfterPhotoList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult GetFileList()
        {
            try
            {
                return PartialView("_FileList", (Session["EditFormModel"] as EditFormModel).FileList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult DeleteBeforePhoto(string AbnormalUniqueID, int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EditFormModel"] as EditFormModel;

                model.BeforePhotoList.Remove(model.BeforePhotoList.First(x => x.AbnormalUniqueID == AbnormalUniqueID && x.Seq == Seq));

                Session["EditFormModel"] = model;

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult DeleteAfterPhoto(string AbnormalUniqueID, int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EditFormModel"] as EditFormModel;

                model.AfterPhotoList.Remove(model.AfterPhotoList.First(x => x.AbnormalUniqueID == AbnormalUniqueID && x.Seq == Seq));

                Session["EditFormModel"] = model;

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult DeleteFile(string AbnormalUniqueID, int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EditFormModel"] as EditFormModel;

                model.FileList.Remove(model.FileList.First(x => x.AbnormalUniqueID == AbnormalUniqueID && x.Seq == Seq));

                Session["EditFormModel"] = model;

                result.Success();
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
        public ActionResult BeforePhotoUpload()
        {
            return PartialView("_BeforePhotoUpload");
        }

        [HttpGet]
        public ActionResult AfterPhotoUpload()
        {
            return PartialView("_AfterPhotoUpload");
        }

        [HttpGet]
        public ActionResult FileUpload()
        {
            return PartialView("_FileUpload");
        }

        [HttpPost]
        public ActionResult UploadBeforePhoto()
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var model = Session["EditFormModel"] as EditFormModel;

                    int seq = 1;

                    if (model.BeforePhotoList.Count > 0)
                    {
                        seq = model.BeforePhotoList.Max(x => x.Seq) + 1;
                    }

                    var photoModel = new PhotoModel()
                    {
                        TempUniqueID = Guid.NewGuid().ToString(),
                        Seq = seq,
                        AbnormalUniqueID = model.UniqueID,
                        Extension = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('.') + 1),
                        Type = "B",
                        IsSaved = false
                    };

                    Request.Files[0].SaveAs(photoModel.TempFullFileName);

                    model.BeforePhotoList.Add(photoModel);

                    result.Success();
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

        [HttpPost]
        public ActionResult UploadAfterPhoto()
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var model = Session["EditFormModel"] as EditFormModel;

                    int seq = 1;

                    if (model.AfterPhotoList.Count > 0)
                    {
                        seq = model.AfterPhotoList.Max(x => x.Seq) + 1;
                    }

                    var photoModel = new PhotoModel()
                    {
                        TempUniqueID = Guid.NewGuid().ToString(),
                        Seq = seq,
                        AbnormalUniqueID = model.UniqueID,
                        Extension = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('.') + 1),
                        Type = "A",
                        IsSaved = false
                    };

                    Request.Files[0].SaveAs(photoModel.TempFullFileName);

                    model.AfterPhotoList.Add(photoModel);

                    result.Success();
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

        [HttpPost]
        public ActionResult UploadFile()
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var model = Session["EditFormModel"] as EditFormModel;

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
                         AbnormalUniqueID=model.UniqueID,
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

        public ActionResult GetRepairFormList()
        {
            try
            {
                return PartialView("_RepairFormList", (Session["EditFormModel"] as EditFormModel).RepairFormList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        [HttpGet]
        public ActionResult CreateRepairForm(string UniqueID)
        {
            try
            {
                RequestResult result = AbnormalHandlingDataAccessor.GetRepairFormCreateFormModel(UniqueID);

                if (result.IsSuccess)
                {
                    return PartialView("_CreateRepairForm", result.Data);
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
        public ActionResult CreateRepairForm(RepairFormCreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                result = AbnormalHandlingDataAccessor.CreateRepairForm(Model, (Session["Account"] as Account).ID);

                if (result.IsSuccess)
                {
                    var model = Session["EditFormModel"] as EditFormModel;

                    var repairFormList = AbnormalHandlingDataAccessor.GetRepairFormList(model.UniqueID);

                    if (repairFormList != null)
                    {
                        model.RepairFormList = repairFormList;

                        Session["EditFormModel"] = model;
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

        public ActionResult InitTree()
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                var account = Session["Account"] as Account;

                RequestResult result = new RequestResult();

                if (account.UserRootOrganizationUniqueID == "*")
                {
                    result = EquipmentDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
                }
                else
                {
                    result = EquipmentDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
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

                RequestResult result = EquipmentDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, Session["Account"] as Account);

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
    }
}