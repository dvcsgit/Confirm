#if ASE
using DataAccess.ASE.QA;
using DataAccess.ASE;
using Models.ASE.QA.CalibrationNotify;
using Models.Authenticated;
using Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;
using System.ComponentModel;
using Webdiyer.WebControls.Mvc;
using System.IO;

namespace WebSite.Areas.Customized_ASE_QA.Controllers
{
    public class CalibrationNotifyController : Controller
    {
        public ActionResult Export()
        {
            RequestResult result = new RequestResult();

            try
            {
                var gridModel = Session["GridViewModel"] as GridViewModel;

                result = CalibrationNotifyDataAccessor.Export(gridModel);

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

        protected IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }

        public ActionResult Index(string VHNO)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = CalibrationNotifyDataAccessor.GetQueryFormModel(organizationList, VHNO);

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

            RequestResult result = CalibrationNotifyDataAccessor.Query(organizationList, Model.Parameters, Session["Account"] as Account);

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

        public ActionResult Detail(string UniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CalibrationNotifyDataAccessor.GetDetailViewModel(organizationList, UniqueID);

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
        public ActionResult Edit(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = CalibrationNotifyDataAccessor.GetEditFormModel(organizationList, UniqueID);

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

                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                result = CalibrationNotifyDataAccessor.Edit(organizationList, model, Session["Account"] as Account);

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

        public ActionResult Manager(string UniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CalibrationNotifyDataAccessor.GetManagerFormModel(organizationList, UniqueID);

                if (result.IsSuccess)
                {
                    return PartialView("_Manager", result.Data);
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

        public ActionResult QA(string UniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CalibrationNotifyDataAccessor.GetQAFormModel(organizationList, UniqueID);

                if (result.IsSuccess)
                {
                    Session["FormModel"] = result.Data;

                    return PartialView("_QA", result.Data);
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

        public ActionResult Approve(ManagerFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
            return Content(JsonConvert.SerializeObject(CalibrationNotifyDataAccessor.Approve(organizationList,Model.UniqueID, Session["Account"] as Account)));
        }

        public ActionResult Reject(ManagerFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
            return Content(JsonConvert.SerializeObject(CalibrationNotifyDataAccessor.Reject(organizationList, Model.UniqueID, Model.FormInput.Comment, Session["Account"] as Account)));
        }

        public ActionResult QAApprove(QAFormModel Model)
        {
            var model = Session["FormModel"] as QAFormModel;

            model.FormInput = Model.FormInput;

            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(CalibrationNotifyDataAccessor.QAApprove(organizationList, model, Session["Account"] as Account)));
        }

        public ActionResult QAReject(QAFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(CalibrationNotifyDataAccessor.QAReject(organizationList, Model.UniqueID, Model.FormInput.Comment, Session["Account"] as Account)));
        }

        public ActionResult GetDetailItemList()
        {
            try
            {
                var model = Session["FormModel"] as EditFormModel;

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
        public ActionResult CreateDetailItem(string IchiUniqueID, string CharacteristicType)
        {
            RequestResult result = CalibrationNotifyDataAccessor.GetDetailItemCreateFormModel(IchiUniqueID, CharacteristicType);

            if (result.IsSuccess)
            {
                Session["DetailFormModel"] = result.Data;

                return PartialView("_CreateDetailItem", Session["DetailFormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult CreateDetailItem(DetailItemCreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["FormModel"] as EditFormModel;

                var detailModel = Session["DetailFormModel"] as DetailItemCreateFormModel;

                detailModel.FormInput = Model.FormInput;

                result = CalibrationNotifyDataAccessor.CreateDetailItem(model.ItemList, detailModel);

                if (result.IsSuccess)
                {
                    Session.Remove("DetailFormModel");

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

        [HttpGet]
        public ActionResult EditDetailItem(int Seq, string IchiUniqueID, string CharacteristicType)
        {
            try
            {
                var model = Session["FormModel"] as EditFormModel;

                RequestResult result = CalibrationNotifyDataAccessor.GetDetailItemEditFormModel(IchiUniqueID, CharacteristicType, model.ItemList.First(x => x.Seq == Seq));

                if (result.IsSuccess)
                {
                    Session["DetailFormModel"] = result.Data;

                    return PartialView("_EditDetailItem", Session["DetailFormModel"]);
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

                var detailModel = Session["DetailFormModel"] as DetailItemEditFormModel;

                detailModel.FormInput = Model.FormInput;

                result = CalibrationNotifyDataAccessor.EditDetailItem(model.ItemList, detailModel);

                if (result.IsSuccess)
                {
                    Session.Remove("DetailFormModel");

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

        public ActionResult DeleteDetailItem(int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["FormModel"] as EditFormModel;

                var item = model.ItemList.First(x => x.Seq == Seq);

                model.ItemList.Remove(item);

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

        public ActionResult GetUserOptions([DefaultValue(1)]int PageIndex, [DefaultValue(10)]int PageSize, string Term, [DefaultValue(false)]bool IsInit)
        {
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = CalibrationNotifyDataAccessor.GetUserOptions(userList, Term, IsInit);

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

        public ActionResult GetManagerID(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                var account = userList.First(x => x.ID == UserID);

                result.ReturnData(account.ManagerID);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
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

        public ActionResult Quatation(string UniqueID)
        {
            var fileName = string.Format("{0}.pdf", UniqueID);

            var filePath = Path.Combine(Config.QAFileFolderPath, fileName);

            return File(filePath, "application/pdf", "報價單.pdf");
        }
    }
}
#endif