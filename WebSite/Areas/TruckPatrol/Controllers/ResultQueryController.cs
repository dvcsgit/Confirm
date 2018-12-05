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
using Models.TruckPatrol.ResultQuery;
using DataAccess.TruckPatrol;

namespace WebSite.Areas.TruckPatrol.Controllers
{
    public class ResultQueryController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel()
            {
                Parameters = new QueryParameters()
                {
                    BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
                    EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today)
                }
            });
        }

        public ActionResult Query(QueryFormModel Model)
        {
            RequestResult result = ResultQueryHelper.Query(Model.Parameters, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["QueryResults"] = result.Data;

                return PartialView("_TruckList", Session["QueryResults"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult ControlPoint(string TruckBindingUniqueID)
        {
            try
            {
                RequestResult result = ResultQueryHelper.Query(TruckBindingUniqueID);

                if (result.IsSuccess)
                {
                    Session["ControlPoints"] = result.Data;
                }

                return PartialView("_ControlPointList", Session["ControlPoints"]);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult CheckItem(string ControlPointUniqueID)
        {
            try
            {
                return PartialView("_CheckItemList", (Session["ControlPoints"] as List<ControlPointModel>).First(x => x.UniqueID == ControlPointUniqueID));
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        //[HttpGet]
        //public ActionResult Edit(string JobUniqueID, string BeginDate, string EndDate)
        //{
        //    RequestResult result = ResultQueryHelper.GetEditFormModel(JobUniqueID, BeginDate, EndDate);

        //    if (result.IsSuccess)
        //    {
        //        return PartialView("_Edit", result.Data);
        //    }
        //    else
        //    {
        //        return PartialView("_Error", result.Error);
        //    }
        //}

        //[HttpPost]
        //public ActionResult Edit(EditFormModel Model)
        //{
        //    return Content(JsonConvert.SerializeObject(ResultQueryHelper.Edit(Model, Session["Account"] as Account)));
        //}

        public ActionResult Export(Define.EnumExcelVersion ExcelVersion)
        {
            var model = ResultQueryHelper.Export(Session["QueryResults"] as List<TruckBindingResultModel>, ExcelVersion).Data as ExcelExportModel;

            return File(model.Data, model.ContentType, model.FileName);
        }

        public ActionResult InitTree()
        {
            try
            {
                RequestResult result = TruckDataAccessor.GetTreeItem("*", Session["Account"] as Account);

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
                RequestResult result = TruckDataAccessor.GetTreeItem(OrganizationUniqueID, Session["Account"] as Account);

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
