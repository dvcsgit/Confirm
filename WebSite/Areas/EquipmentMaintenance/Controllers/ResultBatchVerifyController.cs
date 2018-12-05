using DataAccess.ASE;
using Models.Authenticated;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class ResultBatchVerifyController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Query()
        {
            var itemList = HomeIndexHelper.GetEquipmentPatrolVerifyItemList(Session["Account"] as Account);

            return PartialView("_List", itemList);
        }

        public ActionResult Detail(string UniqueID)
        {
            RequestResult result = ResultVerifyHelper.GetDetailViewModel(UniqueID);

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

                result = ResultVerifyHelper.Confirm(verifyResultList, Session["Account"] as Account);
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