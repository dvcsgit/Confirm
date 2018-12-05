#if GuardPatrol
using DataAccess.GuardPatrol;
using Models.Authenticated;
using Models.GuardPatrol.AbnormalReasonManagement;
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

namespace WebSite.Areas.GuardPatrol.Controllers
{
    public class AbnormalReasonController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = AbnormalReasonDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = AbnormalReasonDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
        public ActionResult Create(string OrganizationUniqueID, string AbnormalType)
        {
            RequestResult result = AbnormalReasonDataAccessor.GetCreateFormModel(OrganizationUniqueID, AbnormalType);

            if (result.IsSuccess)
            {
                Session["AbnormalReasonFormAction"] = Define.EnumFormAction.Create;
                Session["AbnormalReasonCreateFormModel"] = result.Data;

                return PartialView("_Create", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = AbnormalReasonDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["AbnormalReasonFormAction"] = Define.EnumFormAction.Create;
                Session["AbnormalReasonCreateFormModel"] = result.Data;

                return PartialView("_Create", result.Data);
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
                var model = Session["AbnormalReasonCreateFormModel"] as CreateFormModel;

                model.FormInput = Model.FormInput;

                result = AbnormalReasonDataAccessor.Create(model);
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
            RequestResult result = AbnormalReasonDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["AbnormalReasonFormAction"] = Define.EnumFormAction.Edit;
                Session["AbnormalReasonEditFormModel"] = result.Data;

                return PartialView("_Edit", result.Data);
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
                var model = Session["AbnormalReasonEditFormModel"] as EditFormModel;

                model.FormInput = Model.FormInput;

                result = AbnormalReasonDataAccessor.Edit(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult Delete(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                result = AbnormalReasonDataAccessor.Delete(selectedList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult InitTree()
        {
            try
            {
                RequestResult result = AbnormalReasonDataAccessor.GetTreeItem("*", "", Session["Account"] as Account);

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

        public ActionResult GetTreeItem(string OrganizationUniqueID, string AbnormalType)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = AbnormalReasonDataAccessor.GetTreeItem(OrganizationUniqueID, AbnormalType, Session["Account"] as Account);

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

        public ActionResult InitSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                RequestResult result = HandlingMethodDataAccessor.GetTreeItem(RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_SelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string HandlingMethodType)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = HandlingMethodDataAccessor.GetTreeItem(RefOrganizationUniqueID, OrganizationUniqueID, HandlingMethodType);

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

        public ActionResult GetSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["AbnormalReasonFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedList", (Session["AbnormalReasonCreateFormModel"] as CreateFormModel).HandlingMethodList);
                }
                else if ((Define.EnumFormAction)Session["AbnormalReasonFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedList", (Session["AbnormalReasonEditFormModel"] as EditFormModel).HandlingMethodList);
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

        public ActionResult AddSelect(string Selecteds, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                if ((Define.EnumFormAction)Session["AbnormalReasonFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["AbnormalReasonCreateFormModel"] as CreateFormModel;

                    result = AbnormalReasonDataAccessor.AddHandlingMethod(model.HandlingMethodList, selectedList, RefOrganizationUniqueID);

                    if (result.IsSuccess)
                    {
                        model.HandlingMethodList = result.Data as List<HandlingMethodModel>;

                        Session["AbnormalReasonCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["AbnormalReasonFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["AbnormalReasonEditFormModel"] as EditFormModel;

                    result = AbnormalReasonDataAccessor.AddHandlingMethod(model.HandlingMethodList, selectedList, RefOrganizationUniqueID);

                    if (result.IsSuccess)
                    {
                        model.HandlingMethodList = result.Data as List<HandlingMethodModel>;

                        Session["AbnormalReasonEditFormModel"] = model;
                    }
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UnKnownOperation);
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

        public ActionResult DeleteSelected(string HandlingMethodUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                if ((Define.EnumFormAction)Session["AbnormalReasonFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["AbnormalReasonCreateFormModel"] as CreateFormModel;

                    model.HandlingMethodList.Remove(model.HandlingMethodList.First(x => x.UniqueID == HandlingMethodUniqueID));

                    Session["AbnormalReasonCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["AbnormalReasonFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["AbnormalReasonEditFormModel"] as EditFormModel;

                    model.HandlingMethodList.Remove(model.HandlingMethodList.First(x => x.UniqueID == HandlingMethodUniqueID));

                    Session["AbnormalReasonEditFormModel"] = model;

                    result.Success();
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UnKnownOperation);
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