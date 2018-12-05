#if ASE
using DataAccess.ASE.QA;
using DataAccess.ASE;
using Models.ASE.QA.CalibrationForm;
using Models.Authenticated;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;
using Webdiyer.WebControls.Mvc;

namespace WebSite.Areas.Customized_ASE_QA.Controllers
{
    public class CalibrationFormController : Controller
    {
        #region Reviewed
        public ActionResult Index(string Status, string VHNO)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = CalibrationFormDataAccessor.GetQueryFormModel(organizationList, VHNO);

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

            RequestResult result = CalibrationFormDataAccessor.Query(organizationList, Model.Parameters, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["GridViewModel"] = result.Data;

                return PartialView("_List", Session["GridViewModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Verify(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = CalibrationFormDataAccessor.GetVerifyFormModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_Verify", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Detail(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = CalibrationFormDataAccessor.GetDetailViewModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_Detail", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult TakeJob(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(CalibrationFormDataAccessor.TakeJob(UniqueID, Session["Account"] as Account)));
        }

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = CalibrationFormDataAccessor.GetEditFormModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
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

                result = CalibrationFormDataAccessor.Edit(model);

                if (result.IsSuccess)
                {
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

        public ActionResult Return(string UniqueID, string Comment)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(CalibrationFormDataAccessor.Return(organizationList, UniqueID, Comment,Session["Account"] as Account)));
        }

        public ActionResult GetDetailItemList()
        {
            try
            {
                var model = Session["FormModel"] as EditFormModel;

                var canEdit = false;

                if (model.CalibrateType == "IF" || model.CalibrateType == "EL")
                {
                    canEdit = true;
                }
                else
                {
                    if (model.StepLogList.Count > 0)
                    {
                        var last = model.StepLogList.OrderByDescending(x => x.Seq).First();

                        if (last.Step == "1")
                        {
                            canEdit = true;
                        }
                    }
                }

                foreach (var item in model.ItemList)
                {
                    item.CanEdit = canEdit;
                }

                return PartialView("_DetailItemList", model.ItemList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        [HttpGet]
        public ActionResult EditDetailItem(int Seq)
        {
            try
            {
                var model = Session["FormModel"] as EditFormModel;

                var item = model.ItemList.First(x => x.Seq == Seq);

                RequestResult result = CalibrationFormDataAccessor.GetDetailItemEditFormModel(model.UniqueID, item);

                if (result.IsSuccess)
                {
                    Session["DetailItemEditFormModel"] = result.Data;

                    return PartialView("_EditDetailItem", Session["DetailItemEditFormModel"]);
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
        public ActionResult EditDetailItem(DetailItemEditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["FormModel"] as EditFormModel;

                var item = Session["DetailItemEditFormModel"] as DetailItemEditFormModel;

                item.FormInput = Model.FormInput;

                result = CalibrationFormDataAccessor.EditDetailItem(model.ItemList, item);

                if (result.IsSuccess)
                {
                    model.ItemList = result.Data as List<DetailItem>;

                    Session["FormModel"] = model;
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
        public ActionResult Submit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["FormModel"] as EditFormModel;

                model.FormInput = Model.FormInput;

                result = CalibrationFormDataAccessor.Edit(model);

                if (result.IsSuccess)
                {
                    var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                    result = CalibrationFormDataAccessor.Submit(organizationList, model, Session["Account"] as Account);
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

        public ActionResult GetUserOptions([DefaultValue(1)]int PageIndex, [DefaultValue(10)]int PageSize, string Term, [DefaultValue(false)]bool IsInit)
        {
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = CalibrationFormDataAccessor.GetUserOptions(userList, Term, IsInit);

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

        public ActionResult GetSTDUSEOptions([DefaultValue(1)]int PageIndex, [DefaultValue(10)]int PageSize, string Term, [DefaultValue(false)]bool IsInit)
        {
            RequestResult result = CalibrationFormDataAccessor.GetSTDUSEOptions(Term, IsInit, PageIndex, PageSize);

            if (result.IsSuccess)
            {
                var queryResult = result.Data as PagedList<STDUSEModel>;
                //var queryResult = result.Data as PagedList<SelectListItem>;

                var data = queryResult.Select(x => new { id = x.UniqueID, text = string.Format("{0}/上次校驗日期：{1}/下次校驗日期：{2}/校驗人員：{3}", x.CalNo, x.LastCalibrateDateString, x.NextCalibrateDateString, x.Calibrator), name = string.Format("{0}/上次校驗日期：{1}/下次校驗日期：{2}/校驗人員：{3}", x.CalNo, x.LastCalibrateDateString, x.NextCalibrateDateString, x.Calibrator) }).ToList();
                //var data = queryResult.Select(x => new { id = x.Value, text = x.Text, name = x.Text }).AsQueryable().OrderBy(x => x.id).ToPagedList(PageIndex, PageSize);

                return Json(new { Success = true, Data = data, Total = queryResult.TotalItemCount }, JsonRequestBehavior.AllowGet);
                //return Json(new { Success = true, Data = data, Total = data.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, Message = result.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetFileList()
        {
            try
            {
                return PartialView("_FileList", (Session["FormModel"] as EditFormModel).FileList);
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
                var model = Session["FormModel"] as EditFormModel;

                model.FileList.Remove(model.FileList.First(x => x.Seq == Seq));

                Session["FormModel"] = model;

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

        public ActionResult GetEquipmentPhoto()
        {
            try
            {
                return PartialView("_EquipmentPhoto", (Session["FormModel"] as EditFormModel));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        [HttpPost]
        public ActionResult UploadEquipmentPhoto()
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    var fileName = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('\\') + 1);

                    var extension =  fileName.Substring(fileName.LastIndexOf('.') + 1);

                    model.Equipment.Extension = extension;

                    if (System.IO.File.Exists(model.Equipment.PhotoPath))
                    {
                        System.IO.File.Delete(model.Equipment.PhotoPath);
                    }

                    Request.Files[0].SaveAs(model.Equipment.PhotoPath);

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

        public ActionResult GetStepLogList()
        {
            try
            {
                return PartialView("_StepLogList", Session["FormModel"]);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        [HttpGet]
        public ActionResult CreateStepLog(string Step, string CalibrateUnit)
        {
            RequestResult result = CalibrationFormDataAccessor.GetStepLogCreateFormModel(Step, CalibrateUnit);

            if (result.IsSuccess)
            {
                return PartialView("_CreateStepLog", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult CreateStepLog(StepLogCreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["FormModel"] as EditFormModel;

                result = CalibrationFormDataAccessor.CreateStepLog(model.StepLogList, Model,model.CalibrateUnit, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    model.StepLogList = result.Data as List<StepLogModel>;

                    Session["FormModel"] = model;
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

        public ActionResult DeleteStepLog(int Seq)
        {
             RequestResult result = new RequestResult();

             try
             {
                 var model = Session["FormModel"] as EditFormModel;

                 var log = model.StepLogList.First(x => x.Seq == Seq);

                 model.StepLogList.Remove(log);

                 Session["FormModel"] = model;

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

        public ActionResult ExportQRCode(string Selecteds)
        {

            try
            {
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                var fileName = "QRCODE_" + Guid.NewGuid().ToString() + ".xlsx";

                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
                var model = CalibrationFormDataAccessor.ExportQRCode(organizationList, userList, selectedList, Session["Account"] as Account, Define.EnumExcelVersion._2007, fileName) as RequestResult;

                if (model.IsSuccess)
                {
                    var guidFileName = model.Data as string;
                    var desFileName = "QRCODE.xlsx";//depart.Name + "_" + currentDate + ".xlsx";

                    var tempPath = Url.Action("DownloadFile", "Utils", new { guidFileName = guidFileName, desFileName = desFileName });
                    return Json(new { success = true, data = tempPath });
                }
                else
                {
                    ModelState.AddModelError("", model.Message);
                }


            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", ex.Message);
            }

            return Json(new { errors = GetErrorsFromModelState() });
        }

        protected IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }
        #endregion
        
        

       

       
        
        

       

       

       

       

        public ActionResult Approve(VerifyFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(CalibrationFormDataAccessor.Approve(organizationList,Model, Session["Account"] as Account)));
        }

        public ActionResult Reject(VerifyFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(CalibrationFormDataAccessor.Reject(organizationList, Model, Session["Account"] as Account)));
        }



        public ActionResult Download(string FormUniqueID, int Seq)
        {
            var file = CalibrationFormDataAccessor.GetFile(FormUniqueID, Seq);

            return File(file.FullFileName, file.ContentType, file.Display);
        }

        public ActionResult Export(string UniqueID, string CALNO)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            CalibrationFormDataAccessor.GenExcel(organizationList,UniqueID);

            return File(Path.Combine(Config.QAFile_v2FolderPath, string.Format("{0}.xls", UniqueID)), Define.ExcelContentType_2003, string.Format("{0}.xls", CALNO));
        }

        

        public ActionResult RollBack(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(CalibrationFormDataAccessor.Reject(organizationList, new VerifyFormModel()
            {
                UniqueID = UniqueID
            }, Session["Account"] as Account)));
        }

        public ActionResult CalibrationApply(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = DataAccess.ASE.QA.CalibrationApplyDataAccessor.GetDetailViewModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_CalibrationApply", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult CalibrationNotify(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = DataAccess.ASE.QA.CalibrationNotifyDataAccessor.GetDetailViewModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_CalibrationNotify", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }




        public ActionResult GetPhotoList()
        {
            try
            {
                return PartialView("_PhotoList", (Session["DetailItemEditFormModel"] as DetailItemEditFormModel).PhotoList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult DeletePhoto(int CheckItemSeq, int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["DetailItemEditFormModel"] as DetailItemEditFormModel;

                model.PhotoList.Remove(model.PhotoList.First(x => x.CheckItemSeq == CheckItemSeq && x.Seq == Seq));

                Session["DetailItemEditFormModel"] = model;

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
        public ActionResult PhotoUpload()
        {
            return PartialView("_PhotoUpload");
        }

        [HttpPost]
        public ActionResult UploadPhoto()
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var model = Session["DetailItemEditFormModel"] as DetailItemEditFormModel;

                    int seq = 1;

                    if (model.PhotoList.Count > 0)
                    {
                        seq = model.PhotoList.Max(x => x.Seq) + 1;
                    }

                    var photoName = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('\\') + 1);
                    var extension = photoName.Substring(photoName.LastIndexOf('.') + 1);

                    var fileName = string.Format("{0}_{1}_{2}.{3}", model.CalibrationFormUniqueID, model.Seq, seq, extension);

                    var photoModel = new PhotoModel()
                    {
                        FileName = fileName
                    };

                    Request.Files[0].SaveAs(Path.Combine(Config.QAFileFolderPath, fileName));

                    model.PhotoList.Add(photoModel);

                    Session["DetailItemEditFormModel"] = model;

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
    }
}
#endif