#if ASE
using DataAccess.ASE.QS;
using Models.ASE.QS.CheckItemManagement;
using Newtonsoft.Json;
using System.Web.Mvc;
using Utility.Models;

namespace WebSite.Areas.Customized_ASE_QS.Controllers
{
    public class CheckItemController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = CheckItemDataAccessor.Query(Parameters);

            if (result.IsSuccess)
            {
                return PartialView("_List", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Detail(decimal TypeID)
        {
            RequestResult result = CheckItemDataAccessor.GetDetailViewModel(TypeID);

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
            return Content(JsonConvert.SerializeObject(CheckItemDataAccessor.Create(Model)));
        }

        [HttpGet]
        public ActionResult Edit(decimal TypeID)
        {
            RequestResult result = CheckItemDataAccessor.GetEditFormModel(TypeID);

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
            return Content(JsonConvert.SerializeObject(CheckItemDataAccessor.Edit(Model)));
        }

        public ActionResult Delete(decimal TypeID)
        {
            return Content(JsonConvert.SerializeObject(CheckItemDataAccessor.Delete(TypeID)));
        }
    }
}
#endif