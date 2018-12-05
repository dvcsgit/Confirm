using DataAccess.ASE.QS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.Models;

namespace WebSite.Areas.Customized_ASE_QS.Controllers
{
    public class PhotoController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Query()
        {
            RequestResult result = PhotoDataAccessor.Query();

            if (result.IsSuccess)
            {
                return PartialView("_List", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Delete(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(PhotoDataAccessor.Delete(UniqueID)));
        }
    }
}