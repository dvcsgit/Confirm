#if ASE
using DataAccess.ASE.QA;
using DataAccess.ASE;
using Models.ASE.QA.ChangeForm;
using Models.Authenticated;
using Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;
using Webdiyer.WebControls.Mvc;

namespace WebSite.Areas.Customized_ASE_QA.Controllers
{
    public class ChangeFormController : Controller
    {
        public ActionResult Index(string VHNO)
        {
            return View(new QueryFormModel()
            {
                Parameters = new QueryParameters(){
                 VHNO = VHNO
                },
                StatusSelectItemList = new List<SelectListItem>()
                {
                    Define.DefaultSelectListItem(Resources.Resource.SelectAll),
                    new SelectListItem() { Selected = false, Text = Resources.Resource.ChangeFormStatus_1, Value = "1" },
                    new SelectListItem() { Selected = false, Text = Resources.Resource.ChangeFormStatus_2, Value = "2" },
                    new SelectListItem() { Selected = false, Text = Resources.Resource.ChangeFormStatus_3, Value = "3" }
                },
                ChangeTypeSelectItemList = new List<SelectListItem>() 
                { 
                    Define.DefaultSelectListItem(Resources.Resource.SelectAll),
                    new SelectListItem() { Selected = false, Text = Resources.Resource.ChangeFormChangeType_1, Value = "1" },
                    new SelectListItem() { Selected = false, Text = Resources.Resource.ChangeFormChangeType_2, Value = "2" },
                    new SelectListItem() { Selected = false, Text = Resources.Resource.ChangeFormChangeType_3, Value = "3" },
                    new SelectListItem() { Selected = false, Text = Resources.Resource.ChangeFormChangeType_4, Value = "4" },
                    new SelectListItem() { Selected = false, Text = Resources.Resource.ChangeFormChangeType_5, Value = "5" },
                    new SelectListItem() { Selected = false, Text = Resources.Resource.ChangeFormChangeType_6, Value = "6" },
                    new SelectListItem() { Selected = false, Text = Resources.Resource.ChangeFormChangeType_7, Value = "7" },
                    new SelectListItem() { Selected = false, Text = "免MSA", Value = "8" },
                    new SelectListItem() { Selected = false, Text = "變更(校正)週期", Value = "9" },
                    new SelectListItem() { Selected = false, Text = "變更(MSA)週期", Value = "A" },
                    new SelectListItem() { Selected = false, Text = "新增校驗", Value = "B" },
                    new SelectListItem() { Selected = false, Text = "新增MSA", Value = "C" }
                }
            });
        }

        public ActionResult Query(QueryFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = ChangeFormDataAccessor.Query(organizationList, Model.Parameters, Session["Account"] as Account);

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
        public ActionResult Create(string ChangeType)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = ChangeFormDataAccessor.GetCreateFormModel(organizationList, ChangeType, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["ChangeFormCreateFormModel"] = result.Data;

                return PartialView("_Create", Session["ChangeFormCreateFormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpGet]
        public ActionResult CreateByAbnormal(string AbnormalFormUniqueID, string ChangeType)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            RequestResult result = ChangeFormDataAccessor.GetCreateFormModel(organizationList, AbnormalFormUniqueID, ChangeType);

            if (result.IsSuccess)
            {
                return PartialView("_CreateByAbnormal", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model)
        {
            var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

            return Content(JsonConvert.SerializeObject(ChangeFormDataAccessor.Create(organizationList, Model, Session["Account"] as Account)));
        }

        [HttpGet]
        public ActionResult Detail(string UniqueID)
        {
            RequestResult result = ChangeFormDataAccessor.GetDetailViewModel(UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_Detail",result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult GetUserOptions([DefaultValue(1)]int PageIndex, [DefaultValue(10)]int PageSize, string Term, [DefaultValue(false)]bool IsInit)
        {
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = CalibrationApplyDataAccessor.GetUserOptions(userList, Term, IsInit);

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

        public ActionResult GetManagerID(string UserID)
        {
            RequestResult result = new RequestResult();

            try
            {
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                var account = userList.First(x => x.ID == UserID);

                result.ReturnData(account.ManagerID);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult Approve(string UniqueID, int Seq, string Comment)
        {
            return Content(JsonConvert.SerializeObject(ChangeFormDataAccessor.Approve(UniqueID, Seq, Comment, Session["Account"] as Account)));
        }

        public ActionResult Reject(string UniqueID, int Seq, string Comment)
        {
            return Content(JsonConvert.SerializeObject(ChangeFormDataAccessor.Reject(UniqueID, Seq, Comment, Session["Account"] as Account)));
        }

        public ActionResult ExportQRCode(string Selecteds)
        {

            try
            {
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                var fileName = "QRCODE_" + Guid.NewGuid().ToString() + ".xlsx";

                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                var model = ChangeFormDataAccessor.ExportQRCode(organizationList, userList, selectedList, Session["Account"] as Account, Define.EnumExcelVersion._2007, fileName) as RequestResult;

                if (model.IsSuccess)
                {
                    var guidFileName = model.Data as string;
                    var desFileName = "QRCODE.xlsx";//depart.Name + "_" + currentDate + ".xlsx";

                    var tempPath = Url.Action("DownloadFile", "Utils", new { guidFileName = guidFileName, desFileName = desFileName });
                    return Json(new { success = true, data = tempPath });
                }
                else
                {
                    ModelState.AddModelError("", model.Message);
                }


            }
            catch (Exception ex)
            {

                ModelState.AddModelError("", ex.Message);
            }

            return Json(new { errors = GetErrorsFromModelState() });
        }

        protected IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }
    }
}
#endif