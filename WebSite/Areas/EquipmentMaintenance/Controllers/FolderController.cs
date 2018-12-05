using System.Web.Mvc;
using Newtonsoft.Json;
using Utility.Models;
using Models.EquipmentMaintenance.FolderManagement;

#if ASE
using DataAccess.ASE;
#else
using DataAccess.EquipmentMaintenance;
#endif

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class FolderController : Controller
    {
        [HttpGet]
        public ActionResult Create(string OrganizationUniqueID, string EquipmentUniqueID, string PartUniqueID, string MaterialUniqueID, string FolderUniqueID)
        {
            return PartialView("_Create", new CreateFormModel()
            {
                OrganizationUniqueID = OrganizationUniqueID,
                EquipmentUniqueID = EquipmentUniqueID,
                PartUniqueID = PartUniqueID,
                MaterialUniqueID=MaterialUniqueID,
                FolderUniqueID = FolderUniqueID
            });
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(FolderDataAccessor.Create(Model)));
        }

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = FolderDataAccessor.GetEditFormModel(UniqueID);

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
            return Content(JsonConvert.SerializeObject(FolderDataAccessor.Edit(Model)));
        }

        public ActionResult Delete(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(FolderDataAccessor.Delete(UniqueID)));
        }
    }
}