﻿using DataAccess.ASE;
using Models.ASE.QA.MSAReport;
using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.Models;
using Utility;
using DataAccess.ASE.QA;
using System.Reflection;

namespace WebSite.Areas.Customized_ASE_QA.Controllers
{
    public class MSAReportController : Controller
    {
        public ActionResult Index()
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = MSAReportHelper.GetQueryFormModel(organizationList);

            if (result.IsSuccess)
            {
                return View(result.Data);
            }
            else
            {
                ViewBag.Error = result.Error;

                return View("Error");
            }
        }

        public ActionResult Query(QueryFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = MSAReportHelper.Query(organizationList, Model.Parameters, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["QueryResults"] = result.Data;

                return PartialView("_List", Session["QueryResults"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Export()
        {
            RequestResult result = new RequestResult();

            try
            {
                var itemList = Session["QueryResults"] as GridViewModel;

                result = MSAReportHelper.Export(itemList);

                if (result.IsSuccess)
                {
                    var model = result.Data as ExcelExportModel;

                    var tempPath = Url.Action("Download", "Utils", new { FullFileName = model.FullFileName });

                    return Json(new { success = true, data = tempPath });
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Json(new { errors = GetErrorsFromModelState() });
        }

        protected IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }
    }
}