﻿#if ASE
using DataAccess;
using DataAccess.ASE.QA;
using Models.ASE.QA.MSANotify;
using Models.Authenticated;
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
    public class MSANotifyController : Controller
    {
        public ActionResult Index(string VHNO)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = MSANotifyDataAccessor.GetQueryFormModel(organizationList, VHNO);

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

            RequestResult result = MSANotifyDataAccessor.Query(organizationList, Model.Parameters, Session["Account"] as Account);

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

                RequestResult result = MSANotifyDataAccessor.GetDetailViewModel(organizationList, UniqueID);

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

        public ActionResult Manager(string UniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = MSANotifyDataAccessor.GetManagerFormModel(organizationList, UniqueID);

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

                RequestResult result = MSANotifyDataAccessor.GetPEFormModel(organizationList, UniqueID);

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

                RequestResult result = MSANotifyDataAccessor.GetQAFormModel(organizationList, UniqueID);

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
            return Content(JsonConvert.SerializeObject(MSANotifyDataAccessor.Approve(organizationList,Model.UniqueID, Session["Account"] as Account)));
        }

        public ActionResult Reject(ManagerFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(MSANotifyDataAccessor.Reject(organizationList,Model.UniqueID, Model.FormInput.Comment, Session["Account"] as Account)));
        }

        public ActionResult PEApprove(PEFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(MSANotifyDataAccessor.PEApprove(organizationList, Model)));
        }

        public ActionResult PEReject(PEFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(MSANotifyDataAccessor.Reject(organizationList, Model.UniqueID, Model.FormInput.Comment, Session["Account"] as Account)));
        }

        public ActionResult ChangePE(PEFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(MSANotifyDataAccessor.ChangePE(organizationList, Model)));
        }

        public ActionResult QAApprove(QAFormModel Model)
        {
            var model = Session["FormModel"] as QAFormModel;
            model.FormInput = Model.FormInput;

            return Content(JsonConvert.SerializeObject(MSANotifyDataAccessor.QAApprove(model, Session["Account"] as Account)));
        }

        public ActionResult QAReject(QAFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(MSANotifyDataAccessor.QAReject(organizationList, Model.UniqueID, Model.FormInput.Comment, Session["Account"] as Account)));
        }

        public ActionResult MSACharacteristic(string MSAIchiUniqueID)
        {
            RequestResult result = MSANotifyDataAccessor.GetMSACharacteristicList(MSAIchiUniqueID);

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

            RequestResult result = MSANotifyDataAccessor.GetUserOptions(userList, Term, IsInit);

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
    }
}
#endif