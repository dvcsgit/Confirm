#if ASE
using DataAccess.ASE;
using Models.ASE.InventoryManagerManagement;
using Models.Authenticated;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;
using Webdiyer.WebControls.Mvc;

namespace WebSite.Areas.Customized_ASE.Controllers
{
    public class InventoryManagerController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Query()
        {
            RequestResult result = InventoryManagerDataAccessor.Query();

            if (result.IsSuccess)
            {
                return PartialView("_List", result.Data);
            }
            else
            {
                return View("_Error", result.Error);
            }
        }

        public ActionResult Detail(string OrganizationUniqueID)
        {
            RequestResult result = InventoryManagerDataAccessor.GetDetailViewModel(OrganizationUniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_Detail", result.Data);
            }
            else
            {
                return View("_Error", result.Error);
            }
        }

        [HttpGet]
        public ActionResult Create()
        {
            return PartialView("_Create", new CreateFormModel());
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(InventoryManagerDataAccessor.Create(Model)));
        }

        [HttpGet]
        public ActionResult Edit(string OrganizationUniqueID)
        {
            RequestResult result = InventoryManagerDataAccessor.GetEditFormModel(OrganizationUniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_Edit", result.Data);
            }
            else
            {
                return View("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(InventoryManagerDataAccessor.Edit(Model)));
        }

        public ActionResult Delete(string OrganizationUniqueID)
        {
            return Content(JsonConvert.SerializeObject(InventoryManagerDataAccessor.Delete(OrganizationUniqueID)));
        }

        public ActionResult GetUserOptions([DefaultValue(1)]int PageIndex, [DefaultValue(10)]int PageSize, string Term, [DefaultValue(false)]bool IsInit)
        {
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = InventoryManagerDataAccessor.GetUserOptions(userList, Term, IsInit);

            if (result.IsSuccess)
            {
                var queryResult = result.Data as List<SelectListItem>;

                var data = queryResult.Select(x => new { id = x.Value, text = x.Text, name = x.Text }).AsQueryable().OrderBy(x => x.id).ToPagedList(PageIndex, PageSize);

                return Json(new { Success = true, Data = data, Total = data.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, Message = result.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetOrganizationOptions([DefaultValue(1)]int PageIndex, [DefaultValue(10)]int PageSize, string Term, [DefaultValue(false)]bool IsInit)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = InventoryManagerDataAccessor.GetOrganizationOptions(organizationList, Term, IsInit);

            if (result.IsSuccess)
            {
                var queryResult = result.Data as List<SelectListItem>;

                var data = queryResult.Select(x => new { id = x.Value, text = x.Text, name = x.Text }).AsQueryable().OrderBy(x => x.id).ToPagedList(PageIndex, PageSize);

                return Json(new { Success = true, Data = data, Total = data.TotalItemCount }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, Message = result.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
#endif