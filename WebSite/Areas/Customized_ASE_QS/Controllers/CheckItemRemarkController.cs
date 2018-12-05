#if ASE
using DataAccess.ASE.QS;
using Models.ASE.QS.CheckItemRemarkManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.Models;

namespace WebSite.Areas.Customized_ASE_QS.Controllers
{
    public class CheckItemRemarkController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = CheckItemRemarkDataAccessor.Query(Parameters);

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
            RequestResult result = CheckItemRemarkDataAccessor.GetDetailViewModel(UniqueID);

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
            RequestResult result = CheckItemRemarkDataAccessor.GetEditFormModel(UniqueID);

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
            return Content(JsonConvert.SerializeObject(CheckItemRemarkDataAccessor.Edit(Model)));
        }
    }
}
#endif