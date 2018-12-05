#if PipelinePatrol
using DataAccess.PipelinePatrol;
using Models.PipelinePatrol.FolderManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.Models;

namespace WebSite.Areas.PipelinePatrol.Controllers
{
    public class FolderController : Controller
    {
        [HttpGet]
        public ActionResult Create(string OrganizationUniqueID, string PipelineUniqueID, string PipePointUniqueID, string FolderUniqueID)
        {
            return PartialView("_Create", new CreateFormModel()
            {
                OrganizationUniqueID = OrganizationUniqueID,
                PipelineUniqueID = PipelineUniqueID,
                PipePointUniqueID = PipePointUniqueID,
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
#endif