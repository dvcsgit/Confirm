﻿#if ASE
using DataAccess.ASE.QA;
using Models.ASE.QA.UnitManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.Models;

namespace WebSite.Areas.Customized_ASE_QA.Controllers
{
    public class UnitController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = UnitDataAccessor.Query(Parameters);

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
            RequestResult result = UnitDataAccessor.GetDetailViewModel(UniqueID);

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
            return Content(JsonConvert.SerializeObject(UnitDataAccessor.Create(Model)));
        }

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = UnitDataAccessor.GetEditFormModel(UniqueID);

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
            return Content(JsonConvert.SerializeObject(UnitDataAccessor.Edit(Model)));
        }
    }
}
#endif