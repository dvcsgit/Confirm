#if PipelinePatrol
using DataAccess.PipelinePatrol;
using Models.Authenticated;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.PipelinePatrol.Controllers
{
    public class MessageController : Controller
    {
        // GET: PipelinePatrol/Message
        public ActionResult Index()
        {
            RequestResult result = DialogDataAccessor.GetDialog();

            if (result.IsSuccess)
            {
                return View(result.Data);
            }
            else
            {
                ViewBag.Eror = result.Error;

                return View("Error");
            }
        }

        public ActionResult Dialog(string UniqueID)
        {
            RequestResult result = DialogDataAccessor.GetDialog(UniqueID);

            if (result.IsSuccess)
            {
                Session["DialogUniqueID"] = UniqueID;

                return View(result.Data);
            }
            else
            {
                ViewBag.Eror = result.Error;

                return View("Error");
            }
        }

        public ActionResult GetMessage(string DialogUniqueID, int Seq)
        {
            RequestResult result = DialogDataAccessor.GetMessage(DialogUniqueID, Seq);

            if (result.IsSuccess)
            {
                return PartialView("_Messages", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult NewMessage(string DialogUniqueID, string Message)
        {
            return Content(JsonConvert.SerializeObject(DialogDataAccessor.NewMessage(DialogUniqueID, Message, Session["Account"] as Account)));
        }

        [HttpPost]
        public ActionResult Upload()
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var extension = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('.') + 1);

                    var tempFile = Path.Combine(Config.TempFolder, string.Format("{0}.{1}", Guid.NewGuid().ToString(), extension));

                    Request.Files[0].SaveAs(tempFile);

                    result = DialogDataAccessor.NewPhoto(Session["DialogUniqueID"].ToString(), tempFile, extension, Session["Account"] as Account);
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UploadFileRequired);
                }
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }
    }
}
#endif