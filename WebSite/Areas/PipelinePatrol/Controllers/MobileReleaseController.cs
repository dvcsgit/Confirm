#if PipelinePatrol
using DataAccess.PipelinePatrol;
using Models.PipelinePatrol.MobileRelease;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.PipelinePatrol.Controllers
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
#endif