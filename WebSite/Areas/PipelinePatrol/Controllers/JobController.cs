#if PipelinePatrol
using DataAccess;
using DataAccess.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.JobManagement;
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
    public class JobController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = JobDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = JobDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
            Session["JobFormAction"] = Define.EnumFormAction.Create;
            Session["JobCreateFormModel"] = new CreateFormModel()
            {
                OrganizationUniqueID = OrganizationUniqueID,
                ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID),
            };

            return PartialView("_Create", Session["JobCreateFormModel"]);
        }

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = JobDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["JobFormAction"] = Define.EnumFormAction.Create;
                Session["JobCreateFormModel"] = result.Data;

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
                var model = Session["JobCreateFormModel"] as CreateFormModel;

                model.FormInput = Model.FormInput;

                result = JobDataAccessor.Create(model);

                if (result.IsSuccess)
                {
                    Session.Remove("JobFormAction");
                    Session.Remove("JobCreateFormModel");
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
            RequestResult result = JobDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["JobFormAction"] = Define.EnumFormAction.Edit;
                Session["JobEditFormModel"] = result.Data;

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
                var model = Session["JobEditFormModel"] as EditFormModel;

                model.FormInput = Model.FormInput;

                result = JobDataAccessor.Edit(model);

                if (result.IsSuccess)
                {
                    Session.Remove("JobFormAction");
                    Session.Remove("JobEditFormModel");
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

        //public ActionResult Delete(string Selecteds)
        //{
        //    RequestResult result = new RequestResult();

        //    try
        //    {
        //        var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

        //        result = CheckItemDataAccessor.Delete(selectedList);
        //    }
        //    catch (Exception ex)
        //    {
        //        var err = new Error(MethodBase.GetCurrentMethod(), ex);

        //        Logger.Log(err);

        //        result.ReturnError(err);
        //    }

        //    return Content(JsonConvert.SerializeObject(result));
        //}

        public ActionResult InitTree()
        {
            try
            {
                RequestResult result = JobDataAccessor.GetTreeItem("*", Session["Account"] as Account);

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
                RequestResult result = JobDataAccessor.GetTreeItem(OrganizationUniqueID, Session["Account"] as Account);

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
                RequestResult result = RouteDataAccessor.GetTreeItem(RefOrganizationUniqueID, "*");

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

        public ActionResult GetSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = RouteDataAccessor.GetTreeItem(RefOrganizationUniqueID, OrganizationUniqueID);

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
                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedList", (Session["JobCreateFormModel"] as CreateFormModel).RouteList);
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedList", (Session["JobEditFormModel"] as EditFormModel).RouteList);
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

                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["JobCreateFormModel"] as CreateFormModel;

                    result = JobDataAccessor.AddRoute(model.RouteList, selectedList, RefOrganizationUniqueID);

                    if (result.IsSuccess)
                    {
                        model.RouteList = result.Data as List<RouteModel>;

                        Session["JobCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["JobEditFormModel"] as EditFormModel;

                    result = JobDataAccessor.AddRoute(model.RouteList, selectedList, RefOrganizationUniqueID);

                    if (result.IsSuccess)
                    {
                        model.RouteList = result.Data as List<RouteModel>;

                        Session["JobEditFormModel"] = model;
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

        public ActionResult DeleteSelected(string RouteUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["JobCreateFormModel"] as CreateFormModel;

                    model.RouteList.Remove(model.RouteList.First(x => x.UniqueID == RouteUniqueID));

                    Session["JobCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["JobFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["JobEditFormModel"] as EditFormModel;

                    model.RouteList.Remove(model.RouteList.First(x => x.UniqueID == RouteUniqueID));

                    Session["JobEditFormModel"] = model;

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