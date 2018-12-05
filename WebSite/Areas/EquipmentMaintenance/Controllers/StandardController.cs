#if ASE
using DataAccess.ASE;
#else
using DataAccess;
using DataAccess.EquipmentMaintenance;
#endif
using Models.Authenticated;
using Models.EquipmentMaintenance.StandardManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.Models;
using Utility;
using System.Reflection;
using Newtonsoft.Json;
using Models.Shared;


namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class StandardController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = StandardDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = StandardDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
            RequestResult result = StandardDataAccessor.GetCreateFormModel(OrganizationUniqueID, CheckType);

            if (result.IsSuccess)
            {
                Session["StandardFormAction"] = Define.EnumFormAction.Create;
                Session["StandardCreateFormModel"] = result.Data;

                return PartialView("_Create", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = StandardDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["StandardFormAction"] = Define.EnumFormAction.Create;
                Session["StandardCreateFormModel"] = result.Data;

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
                var model = Session["StandardCreateFormModel"] as CreateFormModel;

                model.FormInput = Model.FormInput;

                result = StandardDataAccessor.Create(model);

                if (result.IsSuccess)
                {
                    Session.Remove("StandardFormAction");
                    Session.Remove("StandardCreateFormModel");
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
            RequestResult result = StandardDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["StandardFormAction"] = Define.EnumFormAction.Edit;
                Session["StandardEditFormModel"] = result.Data;

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
                var model = Session["StandardEditFormModel"] as EditFormModel;

                model.FormInput = Model.FormInput;

                result = StandardDataAccessor.Edit(model);

                if (result.IsSuccess)
                {
                    Session.Remove("StandardFormAction");
                    Session.Remove("StandardEditFormModel");
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

                result = StandardDataAccessor.Delete(selectedList);
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
                    result = StandardDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID,"", Session["Account"] as Account);
                }
                else
                {
                    result = StandardDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
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

        public ActionResult GetTreeItem(string OrganizationUniqueID, string MaintenanceType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = StandardDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, MaintenanceType, Session["Account"] as Account);

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
                if ((Define.EnumFormAction)Session["StandardFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedList", (Session["StandardCreateFormModel"] as CreateFormModel).AbnormalReasonList);
                }
                else if ((Define.EnumFormAction)Session["StandardFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedList", (Session["StandardEditFormModel"] as EditFormModel).AbnormalReasonList);
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

                if ((Define.EnumFormAction)Session["StandardFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["StandardCreateFormModel"] as CreateFormModel;

                    result = StandardDataAccessor.AddAbnormalReason(model.AbnormalReasonList, selectedList, RefOrganizationUniqueID);

                    if (result.IsSuccess)
                    {
                        model.AbnormalReasonList = result.Data as List<AbnormalReasonModel>;

                        Session["StandardCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["StandardFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["StandardEditFormModel"] as EditFormModel;

                    result = StandardDataAccessor.AddAbnormalReason(model.AbnormalReasonList, selectedList, RefOrganizationUniqueID);

                    if (result.IsSuccess)
                    {
                        model.AbnormalReasonList = result.Data as List<AbnormalReasonModel>;

                        Session["StandardEditFormModel"] = model;
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
                if ((Define.EnumFormAction)Session["StandardFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["StandardCreateFormModel"] as CreateFormModel;

                    model.AbnormalReasonList.Remove(model.AbnormalReasonList.First(x => x.UniqueID == AbnormalReasonUniqueID));

                    Session["StandardCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["StandardFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["StandardEditFormModel"] as EditFormModel;

                    model.AbnormalReasonList.Remove(model.AbnormalReasonList.First(x => x.UniqueID == AbnormalReasonUniqueID));

                    Session["StandardEditFormModel"] = model;

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