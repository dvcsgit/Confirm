#if GuardPatrol
using System.Web.Mvc;
using System.ComponentModel;
using Newtonsoft.Json;
using Utility.Models;
using Models.GuardPatrol.TimeSpanAbnormalReasonManagement;
using DataAccess.GuardPatrol;

namespace WebSite.Areas.GuardPatrol.Controllers
{
    public class TimeSpanAbnormalReasonController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = TimeSpanAbnormalReasonDataAccessor.Query(Parameters);

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
            RequestResult result = TimeSpanAbnormalReasonDataAccessor.GetDetailViewModel(UniqueID);

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
            return Content(JsonConvert.SerializeObject(TimeSpanAbnormalReasonDataAccessor.Create(Model)));
        }

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = TimeSpanAbnormalReasonDataAccessor.GetEditFormModel(UniqueID);

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
            return Content(JsonConvert.SerializeObject(TimeSpanAbnormalReasonDataAccessor.Edit(Model)));
        }

        public ActionResult Delete(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(TimeSpanAbnormalReasonDataAccessor.Delete(UniqueID)));
        }
    }
}
#endif