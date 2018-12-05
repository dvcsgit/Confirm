#if PipelinePatrol
using DataAccess;
using DataAccess.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.PipelineManagement;
using Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using Utility;
using Utility.Models;
using System.Linq;

namespace WebSite.Areas.PipelinePatrol.Controllers
{
    public class PipelineController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = PipelineDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = PipelineDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
            try
            {
                var organization = OrganizationDataAccessor.GetOrganization(OrganizationUniqueID);

                Session["PipelineFormAction"] = Define.EnumFormAction.Create;
                Session["PipelineCreateFormModel"] = new CreateFormModel()
                {
                    UniqueID = Guid.NewGuid().ToString(),
                    OrganizationUniqueID = OrganizationUniqueID,
                    ParentOrganizationFullDescription = organization.FullDescription
                };

                return PartialView("_Create", Session["PipelineCreateFormModel"]);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model, string SpecPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["PipelineCreateFormModel"] as CreateFormModel;

                var specPageStateList = JsonConvert.DeserializeObject<List<string>>(SpecPageStates);

                result = PipelineDataAccessor.SavePageState(model.SpecList, specPageStateList);

                if (result.IsSuccess)
                {
                    model.SpecList = result.Data as List<SpecModel>;

                    model.FormInput = Model.FormInput;

                    result = PipelineDataAccessor.Create(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("PipelineFormAction");
                        Session.Remove("PipelineCreateFormModel");
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
            RequestResult result = PipelineDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["PipelineFormAction"] = Define.EnumFormAction.Edit;
                Session["PipelineEditFormModel"] = result.Data;

                return PartialView("_Edit", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model, string SpecPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["PipelineEditFormModel"] as EditFormModel;

                var specPageStateList = JsonConvert.DeserializeObject<List<string>>(SpecPageStates);

                result = PipelineDataAccessor.SavePageState(model.SpecList, specPageStateList);

                if (result.IsSuccess)
                {
                    model.SpecList = result.Data as List<SpecModel>;

                    model.FormInput = Model.FormInput;

                    result = PipelineDataAccessor.Edit(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("PipelineFormAction");
                        Session.Remove("PipelineEditFormModel");
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

        public ActionResult InitSpecSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                RequestResult result = PipelineSpecDataAccessor.GetTreeItem(RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_SpecSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSpecSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string Type)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = PipelineSpecDataAccessor.GetTreeItem(RefOrganizationUniqueID, OrganizationUniqueID, Type);

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

        public ActionResult GetSpecSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["PipelineFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SpecSelectedList", (Session["PipelineCreateFormModel"] as CreateFormModel).SpecList);
                }
                else if ((Define.EnumFormAction)Session["PipelineFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SpecSelectedList", (Session["PipelineEditFormModel"] as EditFormModel).SpecList);
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

        public ActionResult AddSpec(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["PipelineFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["PipelineCreateFormModel"] as CreateFormModel;

                    result = PipelineDataAccessor.SavePageState(model.SpecList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.SpecList = result.Data as List<SpecModel>;

                        result = PipelineDataAccessor.AddSpec(model.SpecList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.SpecList = result.Data as List<SpecModel>;

                            Session["PipelineCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["PipelineFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["PipelineEditFormModel"] as EditFormModel;

                    result = PipelineDataAccessor.SavePageState(model.SpecList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.SpecList = result.Data as List<SpecModel>;

                        result = PipelineDataAccessor.AddSpec(model.SpecList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.SpecList = result.Data as List<SpecModel>;

                            Session["PipelineEditFormModel"] = model;
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

        public ActionResult DeleteSpec(string SpecUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["PipelineFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["PipelineCreateFormModel"] as CreateFormModel;

                    result = PipelineDataAccessor.SavePageState(model.SpecList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.SpecList = result.Data as List<SpecModel>;

                        model.SpecList.Remove(model.SpecList.First(x => x.UniqueID == SpecUniqueID));

                        model.SpecList = model.SpecList.OrderBy(x => x.Seq).ToList();

                        int seq = 1;

                        foreach (var spec in model.SpecList)
                        {
                            spec.Seq = seq;

                            seq++;
                        }

                        Session["PipelineCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["PipelineFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["PipelineEditFormModel"] as EditFormModel;

                    result = PipelineDataAccessor.SavePageState(model.SpecList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.SpecList = result.Data as List<SpecModel>;

                        model.SpecList.Remove(model.SpecList.First(x => x.UniqueID == SpecUniqueID));

                        model.SpecList = model.SpecList.OrderBy(x => x.Seq).ToList();

                        int seq = 1;

                        foreach (var spec in model.SpecList)
                        {
                            spec.Seq = seq;

                            seq++;
                        }

                        Session["PipelineEditFormModel"] = model;

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

        public ActionResult InitTree()
        {
            try
            {
                RequestResult result = PipelineDataAccessor.GetTreeItem("*", Session["Account"] as Account);

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
    }
}
#endif