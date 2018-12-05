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
using Models.EquipmentMaintenance.ControlPointManagement;

#if ASE
using DataAccess.ASE;
#else
using DataAccess;
using DataAccess.EquipmentMaintenance;
#endif

using System.Web;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class ControlPointController : Controller
    {
        public ActionResult Index()
        {
            return View(new   QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = ControlPointDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = ControlPointDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
        public ActionResult Create(string OrganizationUniqueID)
        {
            Session["ControlPointFormAction"] = Define.EnumFormAction.Create;
            Session["ControlPointCreateFormModel"] = new CreateFormModel()
            {
                OrganizationUniqueID = OrganizationUniqueID,
                ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID)
            };

            return PartialView("_Create", Session["ControlPointCreateFormModel"]);
        }

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = ControlPointDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["ControlPointFormAction"] = Define.EnumFormAction.Create;
                Session["ControlPointCreateFormModel"] = result.Data;

                return PartialView("_Create", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["ControlPointCreateFormModel"] as CreateFormModel;

                var pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                result = ControlPointDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.CheckItemList = result.Data as List<CheckItemModel>;

                    result = ControlPointDataAccessor.Create(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("ControlPointFormAction");
                        Session.Remove("ControlPointCreateFormModel");
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

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = ControlPointDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["ControlPointFormAction"] = Define.EnumFormAction.Edit;
                Session["ControlPointEditFormModel"] = result.Data;

                return PartialView("_Edit", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["ControlPointEditFormModel"] as EditFormModel;

                var pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                result = ControlPointDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.CheckItemList = result.Data as List<CheckItemModel>;

                    result = ControlPointDataAccessor.Edit(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("ControlPointFormAction");
                        Session.Remove("ControlPointEditFormModel");
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

        public ActionResult Delete(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                result = ControlPointDataAccessor.Delete(selectedList);
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
                    result = ControlPointDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
                }
                else
                {
                    result = ControlPointDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
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

                RequestResult result = ControlPointDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, Session["Account"] as Account);

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

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

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

        public ActionResult GetSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string CheckType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, CheckType);

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
                if ((Define.EnumFormAction)Session["ControlPointFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedList", (Session["ControlPointCreateFormModel"] as CreateFormModel).CheckItemList);
                }
                else if ((Define.EnumFormAction)Session["ControlPointFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedList", (Session["ControlPointEditFormModel"] as EditFormModel).CheckItemList);
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

        public ActionResult AddSelect(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["ControlPointFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["ControlPointCreateFormModel"] as CreateFormModel;

                    result = ControlPointDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.CheckItemList = result.Data as List<CheckItemModel>;

                        result = ControlPointDataAccessor.AddCheckItem(model.CheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.CheckItemList = result.Data as List<CheckItemModel>;

                            Session["ControlPointCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["ControlPointFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["ControlPointEditFormModel"] as EditFormModel;

                    result = ControlPointDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.CheckItemList = result.Data as List<CheckItemModel>;

                        result = ControlPointDataAccessor.AddCheckItem(model.CheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.CheckItemList = result.Data as List<CheckItemModel>;

                            Session["ControlPointEditFormModel"] = model;
                        }
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

        public ActionResult DeleteSelected(string CheckItemUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["ControlPointFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["ControlPointCreateFormModel"] as CreateFormModel;

                    result = ControlPointDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.CheckItemList = result.Data as List<CheckItemModel>;

                        model.CheckItemList.Remove(model.CheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["ControlPointCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["ControlPointFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["ControlPointEditFormModel"] as EditFormModel;

                    result = ControlPointDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.CheckItemList = result.Data as List<CheckItemModel>;

                        model.CheckItemList.Remove(model.CheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["ControlPointEditFormModel"] = model;

                        result.Success();
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

#if ASE
        public ActionResult ExportQRCode(string Selecteds)
        {

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                var fileName = "QRCODE_" + Guid.NewGuid().ToString() + ".xlsx";

                var model = ControlPointDataAccessor.ExportQRCode(selectedList, Session["Account"] as Account, Define.EnumExcelVersion._2007, fileName) as RequestResult;

                if (model.IsSuccess)
                {
                    var guidFileName = model.Data as string;
                    var desFileName = "管制點資料.xlsx";//depart.Name + "_" + currentDate + ".xlsx";

                    var tempPath = Url.Action("DownloadFile", "Utils", new { area = "Customized_ASE_QA", guidFileName = guidFileName, desFileName = desFileName });
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
#endif
        protected IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }
    }
}