#if ASE
using DataAccess.ASE;
using DataAccess.ASE.QA;
using Models.ASE.QA.AbnormalForm;
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
    public class AbnormalFormController : Controller
    {
        public ActionResult GetUserOptions([DefaultValue(1)]int PageIndex, [DefaultValue(10)]int PageSize, string Term, [DefaultValue(false)]bool IsInit)
        {
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = AbnormalFormDataAccessor.GetUserOptions(userList, Term, IsInit);

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


        public ActionResult Index(string VHNO)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = AbnormalFormDataAccessor.GetQueryFormModel(organizationList, VHNO);

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

            RequestResult result = AbnormalFormDataAccessor.Query(organizationList, Model.Parameters, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_List", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpGet]
        public ActionResult Create(string CalibrationFormUniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = AbnormalFormDataAccessor.GetCreateFormModel(organizationList, CalibrationFormUniqueID);

            if (result.IsSuccess)
            {
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

                result = AbnormalFormDataAccessor.Create(model);

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

        public ActionResult Detail(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = AbnormalFormDataAccessor.GetDetailViewModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
                Session["FormModel"] = result.Data;

                return PartialView("_Detail", Session["FormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Adjust(string UniqueID, string Remark)
        {
            return Content(JsonConvert.SerializeObject(AbnormalFormDataAccessor.Adjust(UniqueID, Remark, Session["Account"] as Account)));
        }

        public ActionResult Submit(string UniqueID, string FlowVHNO, string FlowClosedDate)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["FormModel"] as DetailViewModel;

                if (string.IsNullOrEmpty(model.FlowFileExtension))
                {
                    result.ReturnFailedMessage("請上傳附件");
                }
                else
                {
                    result = AbnormalFormDataAccessor.Submit(UniqueID, FlowVHNO,DateTimeHelper.DateStringWithSeperator2DateString(FlowClosedDate), model.FlowFileExtension, Session["Account"] as Account);
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

        public ActionResult ApproveAdjust(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(AbnormalFormDataAccessor.ApproveAdjust(UniqueID, Session["Account"] as Account)));
        }

        public ActionResult RejectAdjust(string UniqueID, string VerifyComment)
        {
            return Content(JsonConvert.SerializeObject(AbnormalFormDataAccessor.RejectAdjust(UniqueID, VerifyComment, Session["Account"] as Account)));
        }

        public ActionResult Approve(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(AbnormalFormDataAccessor.Approve(UniqueID, Session["Account"] as Account)));
        }

        public ActionResult Reject(string UniqueID, string VerifyComment)
        {
            return Content(JsonConvert.SerializeObject(AbnormalFormDataAccessor.Reject(UniqueID, VerifyComment, Session["Account"] as Account)));
        }

        public ActionResult QAApprove(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(AbnormalFormDataAccessor.QAApprove(UniqueID, Session["Account"] as Account)));
        }

        public ActionResult QAReject(string UniqueID, string VerifyComment)
        {
            return Content(JsonConvert.SerializeObject(AbnormalFormDataAccessor.QAReject(UniqueID, VerifyComment, Session["Account"] as Account)));
        }

        [HttpPost]
        public ActionResult UploadFlowFile()
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var model = Session["FormModel"] as DetailViewModel;

                    var fileName = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('\\') + 1);

                    model.FlowFileExtension = fileName.Substring(fileName.LastIndexOf('.') + 1);

                    var filePath = Path.Combine(Config.QAFileFolderPath, string.Format("{0}.{1}", model.UniqueID, model.FlowFileExtension));

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    Request.Files[0].SaveAs(filePath);

                    Session["FormModel"] = model;

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

        public ActionResult GetFileList()
        {
            try
            {
                return PartialView("_FileList", (Session["FormModel"] as CreateFormModel).FileList);
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
                var model = Session["FormModel"] as CreateFormModel;

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

        public ActionResult Download(string FormUniqueID, int Seq)
        {
            var file = AbnormalFormDataAccessor.GetFile(FormUniqueID, Seq);

            return File(file.FullFileName, file.ContentType, file.Display);

            //var model = Session["FormModel"] as DetailViewModel;

            //var file = model.FileList.First(x => x.FormUniqueID == FormUniqueID && x.Seq == Seq);

            //return File(file.FullFileName, file.ContentType, file.Display);
        }
    }
}
#endif