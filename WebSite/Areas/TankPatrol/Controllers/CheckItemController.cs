using System;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.TankPatrol.CheckItemManagement;
using DataAccess.TankPatrol;
using DataAccess;
using System.Web;

namespace WebSite.Areas.TankPatrol.Controllers
{
    public class CheckItemController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = CheckItemDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = CheckItemDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
        public ActionResult Create(string OrganizationUniqueID, string CheckType)
        {
            RequestResult result = CheckItemDataAccessor.GetCreateFormModel(OrganizationUniqueID, CheckType);

            if (result.IsSuccess)
            {
                Session["CheckItemFormAction"] = Define.EnumFormAction.Create;
                Session["CheckItemCreateFormModel"] = result.Data;

                return PartialView("_Create", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = CheckItemDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["CheckItemFormAction"] = Define.EnumFormAction.Create;
                Session["CheckItemCreateFormModel"] = result.Data;

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
                var model = Session["CheckItemCreateFormModel"] as CreateFormModel;

                model.FormInput = Model.FormInput;

                result = CheckItemDataAccessor.Create(model);

                if (result.IsSuccess)
                {
                    Session.Remove("CheckItemFormAction");
                    Session.Remove("CheckItemCreateFormModel");
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
            RequestResult result = CheckItemDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["CheckItemFormAction"] = Define.EnumFormAction.Edit;
                Session["CheckItemEditFormModel"] = result.Data;

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
                var model = Session["CheckItemEditFormModel"] as EditFormModel;

                model.FormInput = Model.FormInput;

                result = CheckItemDataAccessor.Edit(model);

                if (result.IsSuccess)
                {
                    Session.Remove("CheckItemFormAction");
                    Session.Remove("CheckItemEditFormModel");
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

        public ActionResult Delete(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                result = CheckItemDataAccessor.Delete(selectedList);
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
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                var account = Session["Account"] as Account;

                RequestResult result = new RequestResult();

                if (account.UserRootOrganizationUniqueID == "*")
                {
                    result = CheckItemDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, "", Session["Account"] as Account);
                }
                else
                {
                    result = CheckItemDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
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

        public ActionResult GetTreeItem(string OrganizationUniqueID, string CheckType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, CheckType, Session["Account"] as Account);

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
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = AbnormalReasonDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

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

        public ActionResult GetSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string AbnormalType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = AbnormalReasonDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, AbnormalType);

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
                if ((Define.EnumFormAction)Session["CheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedList", (Session["CheckItemCreateFormModel"] as CreateFormModel).AbnormalReasonList);
                }
                else if ((Define.EnumFormAction)Session["CheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedList", (Session["CheckItemEditFormModel"] as EditFormModel).AbnormalReasonList);
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

                if ((Define.EnumFormAction)Session["CheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["CheckItemCreateFormModel"] as CreateFormModel;

                    result = CheckItemDataAccessor.AddAbnormalReason(model.AbnormalReasonList, selectedList, RefOrganizationUniqueID);

                    if (result.IsSuccess)
                    {
                        model.AbnormalReasonList = result.Data as List<AbnormalReasonModel>;

                        Session["CheckItemCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["CheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["CheckItemEditFormModel"] as EditFormModel;

                    result = CheckItemDataAccessor.AddAbnormalReason(model.AbnormalReasonList, selectedList, RefOrganizationUniqueID);

                    if (result.IsSuccess)
                    {
                        model.AbnormalReasonList = result.Data as List<AbnormalReasonModel>;

                        Session["CheckItemEditFormModel"] = model;
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

        public ActionResult DeleteSelected(string AbnormalReasonUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                if ((Define.EnumFormAction)Session["CheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["CheckItemCreateFormModel"] as CreateFormModel;

                    model.AbnormalReasonList.Remove(model.AbnormalReasonList.First(x => x.UniqueID == AbnormalReasonUniqueID));

                    Session["CheckItemCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["CheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["CheckItemEditFormModel"] as EditFormModel;

                    model.AbnormalReasonList.Remove(model.AbnormalReasonList.First(x => x.UniqueID == AbnormalReasonUniqueID));

                    Session["CheckItemEditFormModel"] = model;

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