using System;
using System.IO;
using System.Web.Mvc;
using System.Reflection;
using Utility;
using Utility.Models;
using Models.EquipmentMaintenance.MobileRelease;

#if ASE
using DataAccess.ASE;
#else
using DataAccess.EquipmentMaintenance;
#endif

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class MobileReleaseController : Controller
    {
        [HttpGet]
        public ActionResult Upload()
        {
            return View(new UploadFormModel());
        }

        [HttpPost]
        public ActionResult Upload(UploadFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Model.FormInput.File != null && Model.FormInput.File.ContentLength > 0)
                {
                    Model.TempGuid = Guid.NewGuid().ToString();

                    Model.Extension = Model.FormInput.File.FileName.Substring(Model.FormInput.File.FileName.LastIndexOf('.') + 1);

                    Model.FormInput.ApkName = Model.FormInput.File.FileName.Substring(0, Model.FormInput.File.FileName.LastIndexOf('.'));

                    Model.FormInput.File.SaveAs(Model.TempFile);

                    result = MobileReleaseDataAccessor.Upload(Model);
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UploadFileRequired);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            ViewBag.RequestResult = result;

            return View("Upload", new UploadFormModel());
        }

        [HttpGet]
        public ActionResult Download()
        {
            RequestResult result = MobileReleaseDataAccessor.GetGridViewModel();

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

        public ActionResult FileDownload(int ID)
        {
            var file = MobileReleaseDataAccessor.Get(ID);

            return File(file.FullFileName, file.ContentType, file.FileName);
        }
    }
}
