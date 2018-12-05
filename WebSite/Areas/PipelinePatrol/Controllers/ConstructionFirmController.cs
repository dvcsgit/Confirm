#if PipelinePatrol
using DataAccess.PipelinePatrol;
using Models.PipelinePatrol.ConstructionFirmManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.Models;

namespace WebSite.Areas.PipelinePatrol.Controllers
{
    public class ConstructionFirmController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = ConstructionFirmDataAccessor.Query(Parameters);

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
            RequestResult result = ConstructionFirmDataAccessor.GetDetailViewModel(UniqueID);

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
        public ActionResult Create()
        {
            return PartialView("_Create", new CreateFormModel());
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(ConstructionFirmDataAccessor.Create(Model)));
        }

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = ConstructionFirmDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
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
            return Content(JsonConvert.SerializeObject(ConstructionFirmDataAccessor.Edit(Model)));
        }

        public ActionResult Delete(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(ConstructionFirmDataAccessor.Delete(UniqueID)));
        }
    }
}
#endif