using DataAccess.TankPatrol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.Models;

namespace WebSite.Areas.TankPatrol.Controllers
{
    public class MobileReleaseController : Controller
    {
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