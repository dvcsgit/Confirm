#if GuardPatrol
using DataAccess;
using DataAccess.GuardPatrol;
using Models.Authenticated;
using Models.GuardPatrol.ControlPointManagement;
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
    public class ControlPointController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
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
                RequestResult result = ControlPointDataAccessor.GetTreeItem("*", Session["Account"] as Account);

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
                RequestResult result = ControlPointDataAccessor.GetTreeItem(OrganizationUniqueID, Session["Account"] as Account);

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
                RequestResult result = CheckItemDataAccessor.GetTreeItem(RefOrganizationUniqueID, "*", "");

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
                RequestResult result = CheckItemDataAccessor.GetTreeItem(RefOrganizationUniqueID, OrganizationUniqueID, CheckType);

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
    }
}
#endif