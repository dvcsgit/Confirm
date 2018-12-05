#if PipelinePatrol
using DataAccess.PipelinePatrol;
using Models.PipelinePatrol.PipelineAbnormal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.Models;

namespace WebSite.Areas.PipelinePatrol.Controllers
{
    public class PipelineAbnormalController : Controller
    {
        public ActionResult Index(string UniqueID)
        {
            ViewBag.UniqueID = UniqueID;

            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = PipelineAbnormalDataAccessor.Query(Parameters);

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
            RequestResult result = PipelineAbnormalDataAccessor.GetDetailViewModel(UniqueID);

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