#if PipelinePatrol
using DataAccess;
using DataAccess.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.RouteManagement;
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

namespace WebSite.Areas.PipelinePatrol.Controllers
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
        public ActionResult Create(CreateFormModel Model, string CheckPointPageStates, string CheckPointCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["RouteCreateFormModel"] as CreateFormModel;

                List<string> checkPointPageStateList = JsonConvert.DeserializeObject<List<string>>(CheckPointPageStates);
                List<string> checkPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(CheckPointCheckItemPageStates);

                result = RouteDataAccessor.SavePageState(model.CheckPointList, checkPointPageStateList, checkPointCheckItemPageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.CheckPointList = result.Data as List<CheckPointModel>;

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
        public ActionResult Edit(EditFormModel Model, string CheckPointPageStates, string CheckPointCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["RouteEditFormModel"] as EditFormModel;

                List<string> checkPointPageStateList = JsonConvert.DeserializeObject<List<string>>(CheckPointPageStates);
                List<string> checkPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(CheckPointCheckItemPageStates);

                result = RouteDataAccessor.SavePageState(model.CheckPointList, checkPointPageStateList, checkPointCheckItemPageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.CheckPointList = result.Data as List<CheckPointModel>;

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

        public ActionResult GetDetailTreeItem(string RouteUniqueID, string CheckPointUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = RouteDataAccessor.GetDetailTreeItem(RouteUniqueID, CheckPointUniqueID);

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

        public ActionResult InitCheckPointSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                RequestResult result = CheckPointDataAccessor.GetTreeItem(RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_CheckPointSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetCheckPointSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string PipePointType)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = CheckPointDataAccessor.GetTreeItem(RefOrganizationUniqueID, OrganizationUniqueID, PipePointType);

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
                    return PartialView("_SelectedList", (Session["RouteCreateFormModel"] as CreateFormModel).CheckPointList);
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedList", (Session["RouteEditFormModel"] as EditFormModel).CheckPointList);
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

        public ActionResult AddCheckPoint(string Selecteds, string CheckPointPageStates, string CheckPointCheckItemPageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> checkPointPageStateList = JsonConvert.DeserializeObject<List<string>>(CheckPointPageStates);
                List<string> checkPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(CheckPointCheckItemPageStates);

                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RouteCreateFormModel"] as CreateFormModel;

                    result = RouteDataAccessor.SavePageState(model.CheckPointList, checkPointPageStateList, checkPointCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.CheckPointList = result.Data as List<CheckPointModel>;

                        result = RouteDataAccessor.AddCheckPoint(model.CheckPointList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.CheckPointList = result.Data as List<CheckPointModel>;

                            Session["RouteCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RouteEditFormModel"] as EditFormModel;

                    result = RouteDataAccessor.SavePageState(model.CheckPointList, checkPointPageStateList, checkPointCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.CheckPointList = result.Data as List<CheckPointModel>;

                        result = RouteDataAccessor.AddCheckPoint(model.CheckPointList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.CheckPointList = result.Data as List<CheckPointModel>;

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

        public ActionResult DeleteCheckPoint(string CheckPointUniqueID, string CheckPointPageStates, string CheckPointCheckItemPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> checkPointPageStateList = JsonConvert.DeserializeObject<List<string>>(CheckPointPageStates);
                List<string> checkPointCheckItemPageStateList = JsonConvert.DeserializeObject<List<string>>(CheckPointCheckItemPageStates);

                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RouteCreateFormModel"] as CreateFormModel;

                    result = RouteDataAccessor.SavePageState(model.CheckPointList, checkPointPageStateList, checkPointCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.CheckPointList = result.Data as List<CheckPointModel>;

                        model.CheckPointList.Remove(model.CheckPointList.First(x => x.UniqueID == CheckPointUniqueID));

                        Session["RouteCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RouteEditFormModel"] as EditFormModel;

                    result = RouteDataAccessor.SavePageState(model.CheckPointList, checkPointPageStateList, checkPointCheckItemPageStateList);

                    if (result.IsSuccess)
                    {
                        model.CheckPointList = result.Data as List<CheckPointModel>;

                        model.CheckPointList.Remove(model.CheckPointList.First(x => x.UniqueID == CheckPointUniqueID));

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

        public ActionResult InitPipelineSelectTree()
        {
            try
            {
                RequestResult result = PipelineDataAccessor.GetTreeItem("*", Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_PipelineSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetPipelineSelectTreeItem(string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = PipelineDataAccessor.GetTreeItem(OrganizationUniqueID, Session["Account"] as Account);

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

        public ActionResult GetPipelineSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_PipelineSelectedList", (Session["RouteCreateFormModel"] as CreateFormModel).PipelineList);
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_PipelineSelectedList", (Session["RouteEditFormModel"] as EditFormModel).PipelineList);
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

        public ActionResult AddPipeline(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RouteCreateFormModel"] as CreateFormModel;

                    result = RouteDataAccessor.AddPipeline(model.PipelineList, selectedList, Session["Account"] as Account);

                    if (result.IsSuccess)
                    {
                        model.PipelineList = result.Data as List<PipelineModel>;

                        Session["RouteCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RouteEditFormModel"] as EditFormModel;

                    result = RouteDataAccessor.AddPipeline(model.PipelineList, selectedList, Session["Account"] as Account);

                    if (result.IsSuccess)
                    {
                        model.PipelineList = result.Data as List<PipelineModel>;

                        Session["RouteEditFormModel"] = model;
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

        public ActionResult DeletePipeline(string PipelineUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RouteCreateFormModel"] as CreateFormModel;

                    model.PipelineList.Remove(model.PipelineList.First(x => x.UniqueID == PipelineUniqueID));

                    Session["RouteCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["RouteFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RouteEditFormModel"] as EditFormModel;

                    model.PipelineList.Remove(model.PipelineList.First(x => x.UniqueID == PipelineUniqueID));

                    Session["RouteEditFormModel"] = model;

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