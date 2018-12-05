#if ASE
using DataAccess.ASE;
using DataAccess.ASE.QA;
using Models.ASE.QA.MSAForm_v2;
using Models.Authenticated;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;
using Webdiyer.WebControls.Mvc;

namespace WebSite.Areas.Customized_ASE_QA.Controllers
{
    public class MSAForm_v2Controller : Controller
    {
        public ActionResult GetUserOptions([DefaultValue(1)]int PageIndex, [DefaultValue(10)]int PageSize, string Term, [DefaultValue(false)]bool IsInit)
        {
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = MSAForm_v2DataAccessor.GetUserOptions(userList, Term, IsInit);

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

        public ActionResult Index(string VHNO)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = MSAForm_v2DataAccessor.GetQueryFormModel(organizationList, VHNO);

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

        public ActionResult Query(QueryFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = MSAForm_v2DataAccessor.Query(organizationList, Model.Parameters, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_List", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpGet]
        public ActionResult Detail(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = MSAForm_v2DataAccessor.GetDetailViewModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_Detail", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Import(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var tempFileName = Path.Combine(Config.TempFolder, string.Format("{0}.xls", Guid.NewGuid().ToString()));

                    Request.Files[0].SaveAs(tempFileName);

                    var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                    result = MSAForm_v2DataAccessor.Import(organizationList, UniqueID, tempFileName);
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

        [HttpPost]
        public ActionResult Submit(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(MSAForm_v2DataAccessor.Submit(organizationList, UniqueID, Session["Account"] as Account)));
        }

        public ActionResult Verify(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = MSAForm_v2DataAccessor.GetVerifyFormModel(organizationList, UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_Verify", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Return(string UniqueID)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
            return Content(JsonConvert.SerializeObject(MSAForm_v2DataAccessor.Return(organizationList, UniqueID)));
        }

        public ActionResult Export(string UniqueID, string VHNO)
        {
            var filePath = Path.Combine(Config.QAFile_v2FolderPath, string.Format("{0}.xls", UniqueID));

            if (!System.IO.File.Exists(filePath))
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());
                MSAForm_v2DataAccessor.GenExcel(organizationList, UniqueID);
            }

            return File(Path.Combine(Config.QAFile_v2FolderPath, string.Format("{0}.xls", UniqueID)), Define.ExcelContentType_2003, string.Format("{0}.xls", VHNO));
        }

        public ActionResult Approve(VerifyFormModel Model)
        {
var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(MSAForm_v2DataAccessor.Approve(organizationList, Model, Session["Account"] as Account)));
        }

        public ActionResult Reject(VerifyFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(MSAForm_v2DataAccessor.Reject(organizationList, Model, Session["Account"] as Account)));
        }
    }
}
#endif