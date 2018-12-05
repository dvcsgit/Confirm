#if ASE
using DataAccess.ASE;
using DataAccess.ASE.QA;
using Models.ASE.QA.EquipmentManagement;
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
    public class EquipmentController : Controller
    {
        public ActionResult Index()
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = EquipmentHelper.GetQueryFormModel(organizationList);

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

        public ActionResult Query(QueryFormModel Model, [DefaultValue(1)]int PageIndex, [DefaultValue(50)]int PageSize)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            Session["QueryParameters"] = Model.Parameters;

            RequestResult result = EquipmentHelper.Query(organizationList, Model.Parameters, PageIndex, PageSize, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_List", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult AjaxQuery(QueryParameters Parameters)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            Session["QueryParameters"] = Parameters;

            RequestResult result = EquipmentHelper.Query(organizationList, Parameters, Parameters.PageIndex, Parameters.PageSize, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_List", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult AbnormalForm(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = DataAccess.ASE.QA.AbnormalFormDataAccessor.GetDetailViewModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_AbnormalForm", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
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

        public ActionResult CalibrationForm(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = DataAccess.ASE.QA.CalibrationFormDataAccessor.GetDetailViewModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_CalibrationForm", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult MSANotify(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = DataAccess.ASE.QA.MSANotifyDataAccessor.GetDetailViewModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_MSANotify", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult MSAForm(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = DataAccess.ASE.QA.MSAForm_v2DataAccessor.GetDetailViewModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_MSAForm", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult ChangeForm(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = DataAccess.ASE.QA.ChangeFormDataAccessor.GetDetailViewModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_ChangeForm", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = EquipmentHelper.GetEditFormModel(organizationList, UniqueID);

                if (result.IsSuccess)
                {
                    return PartialView("_Edit", result.Data);
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
            return Content(JsonConvert.SerializeObject(EquipmentHelper.Edit(Model)));
        }

        [HttpGet]
        public ActionResult CAL(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return PartialView("_CAL", new CALFormModel()
            {
                UniqueID = UniqueID,
                Equipment = EquipmentHelper.Get(organizationList, UniqueID)
            });
        }

        [HttpPost]
        public ActionResult CAL(CALFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(EquipmentHelper.CAL(Model, organizationList, Session["Account"] as Account)));
        }

        [HttpGet]
        public ActionResult MSA(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return PartialView("_MSA", new MSAFormModel()
            {
                UniqueID = UniqueID,
                Equipment = EquipmentHelper.Get(organizationList, UniqueID)
            });
        }

        [HttpPost]
        public ActionResult MSA(MSAFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(EquipmentHelper.MSA(Model, organizationList, Session["Account"] as Account)));
        }

        public ActionResult Detail(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return PartialView("_Detail", new DetailViewModel()
            {
                UniqueID = UniqueID,
                Equipment = EquipmentHelper.Get(organizationList,UniqueID),
                CalibrationApply = CalibrationApplyDataAccessor.Query(organizationList, UniqueID),
                CalibrationNotifyList = CalibrationNotifyDataAccessor.Query(organizationList, UniqueID),
                CalibrationFormList = CalibrationFormDataAccessor.Query(organizationList, UniqueID),
                ChangeFormList = ChangeFormDataAccessor.Query(organizationList, UniqueID),
                MSANotifyList = MSANotifyDataAccessor.Query(organizationList, UniqueID),
                MSAFormList = MSAForm_v2DataAccessor.Query(organizationList, UniqueID),
                AbnormalFormList = AbnormalFormDataAccessor.Query(organizationList, UniqueID)
            });
        }

        public ActionResult ExportQRCode(string Selecteds)
        {

            try
            {
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                var fileName = "QRCODE_" + Guid.NewGuid().ToString() + ".xlsx";

                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
                var model = EquipmentHelper.ExportQRCode(organizationList, userList, selectedList, Session["Account"] as Account, Define.EnumExcelVersion._2007, fileName) as RequestResult;

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

        public ActionResult Quatation(string UniqueID)
        {
            var fileName = string.Format("{0}.pdf", UniqueID);

            var filePath = Path.Combine(Config.QAFileFolderPath, fileName);

            return File(filePath, "application/pdf", "報價單.pdf");
        }

        protected IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }

        public ActionResult GetUserOptions([DefaultValue(1)]int PageIndex, [DefaultValue(10)]int PageSize, string Term, [DefaultValue(false)]bool IsInit)
        {
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = CalibrationApplyDataAccessor.GetUserOptions(userList, Term, IsInit);

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

        public ActionResult Export()
        {
            RequestResult result = new RequestResult();

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                var parameters = Session["QueryParameters"] as QueryParameters;

                result = EquipmentHelper.Export(organizationList, parameters, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    var model = result.Data as ExcelExportModel;

                    var tempPath = Url.Action("Download", "Utils", new { FullFileName = model.FullFileName });

                    return Json(new { success = true, data = tempPath });
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Json(new { errors = GetErrorsFromModelState() });
        }

        [HttpPost]
        public ActionResult UploadPhoto(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var photoName = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('\\') + 1);
                    var extension = photoName.Substring(photoName.LastIndexOf('.') + 1);

                    result = EquipmentHelper.UploadPhoto(UniqueID, extension);

                    if (result.IsSuccess)
                    {
                        Request.Files[0].SaveAs(Path.Combine(Config.QAFileFolderPath, string.Format("{0}.{1}", UniqueID, extension)));
                    }

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

        [HttpGet]
        public ActionResult QuatationUpload(string UniqueID)
        {
            Session["UniqueID"] = UniqueID;

            return PartialView("_QuatationUpload", UniqueID);
        }

        [HttpPost]
        public ActionResult QuatationUpload()
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var uniqueID = Session["UniqueID"].ToString();

                    var uploadFileName = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('\\') + 1);
                    var extension = uploadFileName.Substring(uploadFileName.LastIndexOf('.') + 1);

                    if (extension != "pdf")
                    {
                        result.ReturnFailedMessage("系統僅支援pdf檔案上傳");
                    }
                    else
                    {
                        var fileName = string.Format("{0}.pdf", uniqueID);

                        Request.Files[0].SaveAs(Path.Combine(Config.QAFileFolderPath, fileName));

                        result.Success(); 
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
    }
}
#endif