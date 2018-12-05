#if ASE
using DataAccess.ASE.QA;
using DataAccess.ASE;
using Models.ASE.QA.CalibrationApply;
using Models.Authenticated;
using Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;
using Webdiyer.WebControls.Mvc;

namespace WebSite.Areas.Customized_ASE_QA.Controllers
{
    public class CalibrationApplyController : Controller
    {
        public ActionResult Index(string VHNO)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = CalibrationApplyDataAccessor.GetQueryFormModel(organizationList, VHNO);

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

            RequestResult result = CalibrationApplyDataAccessor.Query(organizationList, Model.Parameters, Session["Account"] as Account);

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
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CalibrationApplyDataAccessor.GetDetailViewModel(organizationList, UniqueID);

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
        public ActionResult Create()
        {
            RequestResult result = CalibrationApplyDataAccessor.GetCreateFormModel(Session["Account"] as Account);

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

                result = CalibrationApplyDataAccessor.Create(model, Session["Account"] as Account);

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

            RequestResult result = CalibrationApplyDataAccessor.GetEditFormModel(organizationList, UniqueID);

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

                result = CalibrationApplyDataAccessor.Edit(model, Session["Account"] as Account);

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

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = CalibrationApplyDataAccessor.GetCopyFormModel(UniqueID);

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

        public ActionResult Owner(string UniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CalibrationApplyDataAccessor.GetOwnerFormModel(organizationList, UniqueID);

                if (result.IsSuccess)
                {
                    return PartialView("_Owner", result.Data);
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

        public ActionResult Manager(string UniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CalibrationApplyDataAccessor.GetManagerFormModel(organizationList, UniqueID);

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

        public ActionResult PE(string UniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CalibrationApplyDataAccessor.GetPEFormModel(organizationList, UniqueID);

                if (result.IsSuccess)
                {
                    return PartialView("_PE", result.Data);
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

                RequestResult result = CalibrationApplyDataAccessor.GetQAFormModel(organizationList, UniqueID);

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
            return Content(JsonConvert.SerializeObject(CalibrationApplyDataAccessor.Approve(Model.UniqueID, Session["Account"] as Account)));
        }

        public ActionResult Reject(ManagerFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(CalibrationApplyDataAccessor.Reject(Model.UniqueID, Model.FormInput.Comment, Session["Account"] as Account)));
        }

        public ActionResult PEApprove(PEFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(CalibrationApplyDataAccessor.PEApprove(Model)));
        }

        public ActionResult PEReject(PEFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(CalibrationApplyDataAccessor.Reject(Model.UniqueID, Model.FormInput.Comment, Session["Account"] as Account)));
        }

        public ActionResult ChangePE(PEFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(CalibrationApplyDataAccessor.ChangePE(Model)));
        }

        public ActionResult QAApprove(QAFormModel Model)
        {
            var model = Session["FormModel"] as QAFormModel;

            model.FormInput = Model.FormInput;

            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(CalibrationApplyDataAccessor.QAApprove(organizationList, model, Session["Account"] as Account)));
        }

        public ActionResult QAReject(QAFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(CalibrationApplyDataAccessor.QAReject(Model.UniqueID,Model.FormInput.RejectTo, Model.FormInput.Comment, Session["Account"] as Account)));
        }

        public ActionResult GetDetailItemList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    return PartialView("_DetailItemList", model.ItemList);
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    return PartialView("_DetailItemList", model.ItemList);
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
        public ActionResult CreateDetailItem(string IchiUniqueID, string CharacteristicType)
        {
            RequestResult result = CalibrationApplyDataAccessor.GetDetailItemCreateFormModel(IchiUniqueID, CharacteristicType);

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
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    var detailModel = Session["DetailFormModel"] as DetailItemCreateFormModel;

                    detailModel.FormInput = Model.FormInput;

                    result = CalibrationApplyDataAccessor.CreateDetailItem(model.ItemList, detailModel);

                    if (result.IsSuccess)
                    {
                        Session.Remove("DetailFormModel");

                        model.ItemList = result.Data as List<DetailItem>;

                        Session["FormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    var detailModel = Session["DetailFormModel"] as DetailItemCreateFormModel;

                    detailModel.FormInput = Model.FormInput;

                    result = CalibrationApplyDataAccessor.CreateDetailItem(model.ItemList, detailModel);

                    if (result.IsSuccess)
                    {
                        Session.Remove("DetailFormModel");

                        model.ItemList = result.Data as List<DetailItem>;

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
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    RequestResult result = CalibrationApplyDataAccessor.GetDetailItemEditFormModel(IchiUniqueID, CharacteristicType, model.ItemList.First(x => x.Seq == Seq));

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
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    RequestResult result = CalibrationApplyDataAccessor.GetDetailItemEditFormModel(IchiUniqueID, CharacteristicType, model.ItemList.First(x => x.Seq == Seq));

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

        [HttpPost]
        public ActionResult EditDetailItem(DetailItemEditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    var detailModel = Session["DetailFormModel"] as DetailItemEditFormModel;

                    detailModel.FormInput = Model.FormInput;

                    result = CalibrationApplyDataAccessor.EditDetailItem(model.ItemList, detailModel);

                    if (result.IsSuccess)
                    {
                        Session.Remove("DetailFormModel");

                        model.ItemList = result.Data as List<DetailItem>;

                        Session["FormModel"] = model;

                    }
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    var detailModel = Session["DetailFormModel"] as DetailItemEditFormModel;

                    detailModel.FormInput = Model.FormInput;

                    result = CalibrationApplyDataAccessor.EditDetailItem(model.ItemList, detailModel);

                    if (result.IsSuccess)
                    {
                        Session.Remove("DetailFormModel");

                        model.ItemList = result.Data as List<DetailItem>;

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
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    var item = model.ItemList.First(x => x.Seq == Seq);

                    model.ItemList.Remove(item);

                    Session["FormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    var item = model.ItemList.First(x => x.Seq == Seq);

                    model.ItemList.Remove(item);

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

        public ActionResult MSACharacteristic(string MSAIchiUniqueID)
        {
            RequestResult result = CalibrationApplyDataAccessor.GetMSACharacteristicList(MSAIchiUniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_MSACharacteristic", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
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

        public ActionResult Delete(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(CalibrationApplyDataAccessor.Delete(UniqueID)));
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
    }
}
#endif