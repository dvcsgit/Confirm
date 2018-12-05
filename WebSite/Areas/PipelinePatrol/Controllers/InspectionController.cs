#if PipelinePatrol
using DataAccess.PipelinePatrol;
using Models.PipelinePatrol.Inspection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.Models;

namespace WebSite.Areas.PipelinePatrol.Controllers
{
    public class InspectionController : Controller
    {
        public ActionResult Index(string UniqueID)
        {
            ViewBag.UniqueID = UniqueID;

            RequestResult result = InspectionDataAccessor.GetQueryFormModel();

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

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = InspectionDataAccessor.Query(Parameters);

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
            RequestResult result = InspectionDataAccessor.GetDetailViewModel(UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_Detail", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }
    }
}
#endif