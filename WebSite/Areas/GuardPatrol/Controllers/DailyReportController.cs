#if GuardPatrol
using DataAccess.GuardPatrol;
using Microsoft.Reporting.WebForms;
using Models.Authenticated;
using Models.GuardPatrol.DailyReport;
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
using WebSite.Areas.GuardPatrol.Views.DailyReport.Report;
using WebSite.Reports;

namespace WebSite.Areas.GuardPatrol.Controllers
{
    public class DailyReportController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel()
            {
                Parameters = new QueryParameters()
                {
                    IsOnlyChecked = true,
                    BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today),
                    EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today)
                }
            });
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = DailyReportHelper.Query(Parameters, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["QueryResult"] = result.Data;

                return PartialView("_List", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Export(string FileType)
        {
            var itemList = (Session["QueryResult"] as List<GridItem>);

            DailyReportDataSet ds = new DailyReportDataSet();

            foreach (var item in itemList)
            {
                ds.DataTable.AddDataTableRow(item.CheckDate, item.CheckUser, item.ID, item.Name, item.CheckTime, item.Remark, item.OrganizationDescription);
            }

            ReportWrapper rw = new ReportWrapper();

            rw.FileName = string.Format("巡邏日報表");

            rw.ReportPath = @"D:\FEM\WebSite\Areas\GuardPatrol\Views\DailyReport\Report\DailyReport.rdlc";
            //rw.ReportPath = @"D:\Project\FEM\Source Code\FEM\WebSite\Areas\GuardPatrol\Views\DailyReport\Report\DailyReport.rdlc";
            //rw.ReportPath = Url.Content("~/Areas/GuardPatrol/Views/DailyReport/Report/DailyReport.rdlc");

            rw.ReportDataSources.Add(new ReportDataSource("DataSet_DailyReport", ds.DataTable.Copy()));
            rw.ReportParameters.Add(new ReportParameter("ReportTime", string.Format("{0}/{1}", (DateTime.Now.Year - 1911).ToString(), DateTime.Now.ToString("MM/dd HH:mm:ss"))));

            rw.IsDownloadDirectly = true;
            rw.DownloadType = FileType;

            Session["ReportWrapper"] = rw;

            return PartialView("_Export");
        }

        public ActionResult InitTree()
        {
            try
            {
                RequestResult result = DailyReportHelper.GetTreeItem("*", Session["Account"] as Account);

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
                RequestResult result = DailyReportHelper.GetTreeItem(OrganizationUniqueID, Session["Account"] as Account);

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