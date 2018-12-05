#if GuardPatrol
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebSite.Areas.GuardPatrol.Controllers
{
    public class ResultQueryController : Controller
    {
        //public ActionResult Index()
        //{
        //    return View(new QueryFormModel()
        //    {
        //        Parameters = new QueryParameters()
        //        {
        //            BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
        //            EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today)
        //        }
        //    });
        //}

        //public ActionResult Query(QueryFormModel Model)
        //{
        //    RequestResult result = ResultQueryHelper.Query(Model.Parameters, Session["Account"] as Account);

        //    if (result.IsSuccess)
        //    {
        //        Session["QueryResults"] = result.Data;

        //        return PartialView("_JobList", Session["QueryResults"]);
        //    }
        //    else
        //    {
        //        return PartialView("_Error", result.Error);
        //    }
        //}

        //public ActionResult Detail(string JobUniqueID, string CheckDate, string ArriveRecordUniqueID)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(ArriveRecordUniqueID))
        //        {
        //            return PartialView("_CheckResultList", (Session["QueryResults"] as List<JobModel>).First(x => x.JobUniqueID == JobUniqueID && x.CheckDate == CheckDate).ArriveRecordList.First(x => x.UniqueID == ArriveRecordUniqueID));
        //        }
        //        else if (!string.IsNullOrEmpty(JobUniqueID))
        //        {
        //            return PartialView("_ArriveRecordList", (Session["QueryResults"] as List<JobModel>).First(x => x.JobUniqueID == JobUniqueID && x.CheckDate == CheckDate));
        //        }
        //        else
        //        {
        //            return PartialView("_JobList", Session["QueryResults"]);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var err = new Error(MethodBase.GetCurrentMethod(), ex);

        //        Logger.Log(err);

        //        return PartialView("_Error", err);
        //    }
        //}

        //public ActionResult InitTree()
        //{
        //    try
        //    {
        //        RequestResult result = RouteDataAccessor.GetTreeItem("*", "", Session["Account"] as Account);

        //        if (result.IsSuccess)
        //        {
        //            return PartialView("_Tree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
        //        }
        //        else
        //        {
        //            return PartialView("_Error", result.Error);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        var err = new Error(MethodBase.GetCurrentMethod(), ex);

        //        Logger.Log(err);

        //        return PartialView("_Error", err);
        //    }
        //}

        //public ActionResult GetTreeItem(string OrganizationUniqueID, string RouteUniqueID)
        //{
        //    string jsonTree = string.Empty;

        //    try
        //    {
        //        RequestResult result = RouteDataAccessor.GetTreeItem(OrganizationUniqueID, RouteUniqueID, Session["Account"] as Account);

        //        if (result.IsSuccess)
        //        {
        //            jsonTree = JsonConvert.SerializeObject((List<TreeItem>)result.Data);
        //        }
        //        else
        //        {
        //            jsonTree = string.Empty;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Log(MethodBase.GetCurrentMethod(), ex);

        //        jsonTree = string.Empty;
        //    }

        //    return Content(jsonTree);
        //}
    }
}
#endif