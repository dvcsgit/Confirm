#if PipelinePatrol
using DataAccess.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.CheckPointManagement;
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
    public class CheckPointController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = CheckPointDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = CheckPointDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = CheckPointDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["CheckPointFormModel"] = result.Data;

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
                var model = Session["CheckPointFormModel"] as EditFormModel;

                var pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                result = CheckPointDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.CheckItemList = result.Data as List<CheckItemModel>;

                    result = CheckPointDataAccessor.Edit(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("CheckPointFormModel");
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
                RequestResult result = CheckPointDataAccessor.GetTreeItem("*", "", Session["Account"] as Account);

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

        public ActionResult GetTreeItem(string OrganizationUniqueID, string PipePointType)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = CheckPointDataAccessor.GetTreeItem(OrganizationUniqueID, PipePointType, Session["Account"] as Account);

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
                return PartialView("_SelectedList", (Session["CheckPointFormModel"] as EditFormModel).CheckItemList);
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

                var model = Session["CheckPointFormModel"] as EditFormModel;

                result = CheckPointDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                if (result.IsSuccess)
                {
                    model.CheckItemList = result.Data as List<CheckItemModel>;

                    result = CheckPointDataAccessor.AddCheckItem(model.CheckItemList, selectedList, RefOrganizationUniqueID);

                    if (result.IsSuccess)
                    {
                        model.CheckItemList = result.Data as List<CheckItemModel>;

                        Session["CheckPointFormModel"] = model;
                    }
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

                var model = Session["CheckPointFormModel"] as EditFormModel;

                result = CheckPointDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                if (result.IsSuccess)
                {
                    model.CheckItemList = result.Data as List<CheckItemModel>;

                    model.CheckItemList.Remove(model.CheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                    Session["CheckPointFormModel"] = model;

                    result.Success();
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