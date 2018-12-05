#if ASE
using DataAccess.ASE.QA;
using Models.Authenticated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.Customized_ASE_QA.Controllers
{
    public class CalendarController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetEvents(string start, string end)
        {
            try
            {
                RequestResult result = CalendarDataAccessor.GetEvents(DateTimeHelper.DateStringWithSeperator2DateTime(start).Value, DateTimeHelper.DateStringWithSeperator2DateTime(end).Value, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return Json(result.Data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(result.Message);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return Json(err.ErrorMessage, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
#endif