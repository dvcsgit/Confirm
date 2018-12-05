using DataAccess.ASE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility.Models;
using Utility;
using Models.Authenticated;
using System.Reflection;
using Newtonsoft.Json;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class MFormVerifyController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Query()
        {
            var accountList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            var itemList = HomeIndexHelper.GetMaintenanceFormVerifyItemList(accountList, Session["Account"] as Account);

            return PartialView("_List", itemList);
        }

        public ActionResult Detail(string UniqueID)
        {
            var accountList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = MaintenanceFormDataAccessor.GetDetailViewModel(UniqueID, accountList);

            if (result.IsSuccess)
            {
                return PartialView("_Detail", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Verify(string VerifyResults)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> verifyResultList = JsonConvert.DeserializeObject<List<string>>(VerifyResults);

                result = MaintenanceFormDataAccessor.Confirm(verifyResultList, Session["Account"] as Account);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }
    }
}