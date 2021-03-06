﻿#if ASE
using DataAccess.ASE.QS;
using Models.ASE.QS.RemarkManagement;
using Newtonsoft.Json;
using System.Web.Mvc;
using Utility.Models;

namespace WebSite.Areas.Customized_ASE_QS.Controllers
{
    public class RemarkController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = RemarkDataAccessor.Query(Parameters);

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
            RequestResult result = RemarkDataAccessor.GetDetailViewModel(UniqueID);

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
            return Content(JsonConvert.SerializeObject(RemarkDataAccessor.Create(Model)));
        }

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = RemarkDataAccessor.GetEditFormModel(UniqueID);

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
            return Content(JsonConvert.SerializeObject(RemarkDataAccessor.Edit(Model)));
        }

        public ActionResult Delete(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(RemarkDataAccessor.Delete(UniqueID)));
        }
    }
}
#endif