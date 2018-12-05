using System;
using System.Web.Mvc;
using System.Reflection;
using Utility;
using Utility.Models;
using Models.Authenticated;

#if ASE
using DataAccess.ASE;
#else
using DataAccess.EquipmentMaintenance;
#endif

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class CalendarController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetEvents(bool Patrol, bool MaintenanceForm, bool RepairForm, string start, string end)
        {
            try
            {
                RequestResult result = CalendarDataAccessor.GetEvents(Patrol, MaintenanceForm, RepairForm, DateTimeHelper.DateStringWithSeperator2DateTime(start).Value, DateTimeHelper.DateStringWithSeperator2DateTime(end).Value, Session["Account"] as Account);

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