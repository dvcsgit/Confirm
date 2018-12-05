#if GuardPatrol
using DataAccess;
using DataAccess.GuardPatrol;
using Models.Authenticated;
using Models.GuardPatrol.RouteManagement;
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
    public class RouteController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = RouteDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = RouteDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
            Session["RouteFormAction"] = Define.EnumFormAction.Create;
            Session["RouteCreateFormModel"] = new CreateFormModel()
            {
                OrganizationUniqueID = OrganizationUniqueID,
                ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID)
            };

            return PartialView("_Create", Session["RouteCreateFormModel"]);
        }

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = RouteDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["RouteFormAction"] = Define.EnumFormAction.Create;
                Session["RouteCreateFormModel"] = result.Data;

                return PartialView("_Create", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model, string ControlPointPageStates, string ControlPointCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["RouteCreateFormModel"] as CreateFormModel;

                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);

                result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.ControlPointList = result.Data as List<ControlPointModel>;

                    result = RouteDataAccessor.Create(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("RouteFormAction");
                        Session.Remove("RouteCreateFormModel");
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
            RequestResult result = RouteDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["RouteFormAction"] = Define.EnumFormAction.Edit;
                Session["RouteEditFormModel"] = result.Data;

                return PartialView("_Edit", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model, string ControlPointPageStates, string ControlPointCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["RouteEditFormModel"] as EditFormModel;

                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);

                result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.ControlPointList = result.Data as List<ControlPointModel>;

                    result = RouteDataAccessor.Edit(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("RouteFormAction");
                        Session.Remove("RouteEditFormModel");
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

        public ActionResult InitTree()
        {
            try
            {
                RequestResult result = RouteDataAccessor.GetTreeItem("*", Session["Account"] as Account);

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
                RequestResult result = RouteDataAccessor.GetTreeItem(OrganizationUniqueID, Session["Account"] as Account);

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

        public ActionResult InitDetailTree(string UniqueID)
        {
            try
            {
                RequestResult result = RouteDataAccessor.GetDetailTreeItem(UniqueID);

                if (result.IsSuccess)
                {
                    return PartialView("_DetailTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetDetailTreeItem(string RouteUniqueID, string ControlPointUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = RouteDataAccessor.GetDetailTreeItem(RouteUniqueID, ControlPointUniqueID);

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

        public ActionResult InitControlPointSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                RequestResult result = ControlPointDataAccessor.GetTreeItem(RefOrganizationUniqueID, "*");

                if (result.IsSuccess)
                {
                    return PartialView("_ControlPointSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetControlPointSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = ControlPointDataAccessor.GetTreeItem(RefOrganizationUniqueID, OrganizationUniqueID);

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
                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedList", (Session["RouteCreateFormModel"] as CreateFormModel).ControlPointList);
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedList", (Session["RouteEditFormModel"] as EditFormModel).ControlPointList);
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

        public ActionResult AddControlPoint(string Selecteds, string ControlPointPageStates, string ControlPointCheckItemPageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);

                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RouteCreateFormModel"] as CreateFormModel;

                    result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.ControlPointList = result.Data as List<ControlPointModel>;

                        result = RouteDataAccessor.AddControlPoint(model.ControlPointList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.ControlPointList = result.Data as List<ControlPointModel>;

                            Session["RouteCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RouteEditFormModel"] as EditFormModel;

                    result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.ControlPointList = result.Data as List<ControlPointModel>;

                        result = RouteDataAccessor.AddControlPoint(model.ControlPointList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.ControlPointList = result.Data as List<ControlPointModel>;

                            Session["RouteEditFormModel"] = model;
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

        public ActionResult DeleteControlPoint(string ControlPointUniqueID, string ControlPointPageStates, string ControlPointCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> controlPointPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointPageStates);
                List<string> controlPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(ControlPointCheckItemPageStates);

                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RouteCreateFormModel"] as CreateFormModel;

                    result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.ControlPointList = result.Data as List<ControlPointModel>;

                        model.ControlPointList.Remove(model.ControlPointList.First(x => x.UniqueID == ControlPointUniqueID));

                        Session["RouteCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RouteEditFormModel"] as EditFormModel;

                    result = RouteDataAccessor.SavePageState(model.ControlPointList, controlPointPageStateList, controlPointCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.ControlPointList = result.Data as List<ControlPointModel>;

                        model.ControlPointList.Remove(model.ControlPointList.First(x => x.UniqueID == ControlPointUniqueID));

                        Session["RouteEditFormModel"] = model;

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

        public ActionResult Delete(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                result = RouteDataAccessor.Delete(selectedList);
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