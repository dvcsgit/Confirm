using System;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.MaterialManagement;

#if ASE
using DataAccess.ASE;
#else
using DataAccess.EquipmentMaintenance;
using DataAccess;
#endif

using System.Web;
using System.IO;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class MaterialController : Controller
    {
        public ActionResult DeletePhoto(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(MaterialDataAccessor.DeletePhoto(UniqueID)));
        }

        [HttpPost]
        public ActionResult UploadPhoto(string UniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var photoName = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('\\') + 1);
                    var extension = photoName.Substring(photoName.LastIndexOf('.') + 1);

                    result = MaterialDataAccessor.UploadPhoto(UniqueID, extension);

                    if (result.IsSuccess)
                    {
                        Request.Files[0].SaveAs(Path.Combine(Config.EquipmentMaintenancePhotoFolderPath, string.Format("{0}.{1}", UniqueID, extension)));
                    }

                    result.Success();
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

        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = MaterialDataAccessor.Query(Parameters, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_List", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Detail(string UniqueID)
        {
            RequestResult result = MaterialDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                return PartialView("_Detail", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpGet]
        public ActionResult Create(string OrganizationUniqueID, string EquipmentType)
        {
            RequestResult result = MaterialDataAccessor.GetCreateFormModel(OrganizationUniqueID, EquipmentType);

            if (result.IsSuccess)
            {
                Session["MaterialFormAction"] = Define.EnumFormAction.Create;
                Session["MaterialCreateFormModel"] = result.Data;

                return PartialView("_Create", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = MaterialDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["MaterialFormAction"] = Define.EnumFormAction.Create;
                Session["MaterialCreateFormModel"] = result.Data;

                return PartialView("_Create", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["MaterialCreateFormModel"] as CreateFormModel;

                var pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                result = MaterialDataAccessor.SavePageState(model.SpecList, pageStateList);

                if (result.IsSuccess)
                {
                    model.SpecList = result.Data as List<SpecModel>;

                    model.FormInput = Model.FormInput;

                    result = MaterialDataAccessor.Create(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("MaterialFormAction");
                        Session.Remove("MaterialCreateFormModel");
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = MaterialDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["MaterialFormAction"] = Define.EnumFormAction.Edit;
                Session["MaterialEditFormModel"] = result.Data;

                return PartialView("_Edit", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["MaterialEditFormModel"] as EditFormModel;

                var pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                result = MaterialDataAccessor.SavePageState(model.SpecList, pageStateList);

                if (result.IsSuccess)
                {
                    model.SpecList = result.Data as List<SpecModel>;

                    model.FormInput = Model.FormInput;

                    result = MaterialDataAccessor.Edit(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("MaterialFormAction");
                        Session.Remove("MaterialEditFormModel");
                    }
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult Delete(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                result = MaterialDataAccessor.Delete(selectedList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult InitTree()
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                var account = Session["Account"] as Account;

                RequestResult result = new RequestResult();

                if (account.UserRootOrganizationUniqueID == "*")
                {
                    result = MaterialDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID,"", Session["Account"] as Account);
                }
                else
                {
                    result = MaterialDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
                }

                if (result.IsSuccess)
                {
                    return PartialView("_Tree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
                }
                else
                {
                    return PartialView("_Error", result.Error);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult GetTreeItem(string OrganizationUniqueID, string EquipmentType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = MaterialDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, EquipmentType, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    jsonTree = JsonConvert.SerializeObject((List<TreeItem>)result.Data);
                }
                else
                {
                    jsonTree = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                jsonTree = string.Empty;
            }

            return Content(jsonTree);
        }

        public ActionResult InitSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = MaterialSpecDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_SelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
                }
                else
                {
                    return PartialView("_Error", result.Error);
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult GetSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string MaterialType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = MaterialSpecDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, MaterialType);

                if (result.IsSuccess)
                {
                    jsonTree = JsonConvert.SerializeObject((List<TreeItem>)result.Data);
                }
                else
                {
                    jsonTree = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                jsonTree = string.Empty;
            }

            return Content(jsonTree);
        }

        public ActionResult GetSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["MaterialFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedList", (Session["MaterialCreateFormModel"] as CreateFormModel).SpecList);
                }
                else if ((Define.EnumFormAction)Session["MaterialFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedList", (Session["MaterialEditFormModel"] as EditFormModel).SpecList);
                }
                else
                {
                    return PartialView("_Error", new Error(MethodBase.GetCurrentMethod(), Resources.Resource.UnKnownOperation));
                }
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult AddSelect(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["MaterialFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["MaterialCreateFormModel"] as CreateFormModel;

                    result = MaterialDataAccessor.SavePageState(model.SpecList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.SpecList = result.Data as List<SpecModel>;

                        result = MaterialDataAccessor.AddSpec(model.SpecList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.SpecList = result.Data as List<SpecModel>;

                            Session["MaterialCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["MaterialFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["MaterialEditFormModel"] as EditFormModel;

                    result = MaterialDataAccessor.SavePageState(model.SpecList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.SpecList = result.Data as List<SpecModel>;

                        result = MaterialDataAccessor.AddSpec(model.SpecList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.SpecList = result.Data as List<SpecModel>;

                            Session["MaterialEditFormModel"] = model;
                        }
                    }
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UnKnownOperation);
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

        public ActionResult DeleteSelected(string SpecUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["MaterialFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["MaterialCreateFormModel"] as CreateFormModel;

                    result = MaterialDataAccessor.SavePageState(model.SpecList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.SpecList = result.Data as List<SpecModel>;

                        model.SpecList.Remove(model.SpecList.First(x => x.UniqueID == SpecUniqueID));

                        model.SpecList = model.SpecList.OrderBy(x => x.Seq).ToList();

                        int seq = 1;

                        foreach (var spec in model.SpecList)
                        {
                            spec.Seq = seq;

                            seq++;
                        }

                        Session["MaterialCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["MaterialFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["MaterialEditFormModel"] as EditFormModel;

                    result = MaterialDataAccessor.SavePageState(model.SpecList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.SpecList = result.Data as List<SpecModel>;

                        model.SpecList.Remove(model.SpecList.First(x => x.UniqueID == SpecUniqueID));

                        model.SpecList = model.SpecList.OrderBy(x => x.Seq).ToList();

                        int seq = 1;

                        foreach (var spec in model.SpecList)
                        {
                            spec.Seq = seq;

                            seq++;
                        }

                        Session["MaterialEditFormModel"] = model;

                        result.Success();
                    }
                }
                else
                {
                    result.ReturnFailedMessage(Resources.Resource.UnKnownOperation);
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

#if ASE
        public ActionResult ExportQRCode(string Selecteds)
        {

            try
            {
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                var fileName = "QRCODE_" + Guid.NewGuid().ToString() + ".xlsx";

                var model = MaterialDataAccessor.ExportQRCode(userList, selectedList, Session["Account"] as Account, Define.EnumExcelVersion._2007, fileName) as RequestResult;

                if (model.IsSuccess)
                {
                    var guidFileName = model.Data as string;
                    var desFileName = "材料資料.xlsx";//depart.Name + "_" + currentDate + ".xlsx";

                    var tempPath = Url.Action("DownloadFile", "Utils", new { area = "Customized_ASE_QA", guidFileName = guidFileName, desFileName = desFileName });
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
#endif
        protected IEnumerable<string> GetErrorsFromModelState()
        {
            return ModelState.SelectMany(x => x.Value.Errors.Select(error => error.ErrorMessage));
        }
    }
}