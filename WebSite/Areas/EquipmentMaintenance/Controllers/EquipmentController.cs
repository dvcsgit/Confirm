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
using Models.EquipmentMaintenance.EquipmentManagement;
using Report.EquipmentMaintenance.DataAccess;

#if ASE
using DataAccess.ASE;
#else
using DataAccess;
using DataAccess.EquipmentMaintenance;
#endif

using System.Web;
using System.IO;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class EquipmentController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = EquipmentDataAccessor.Query(Parameters, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["QueryResults"] = result.Data;
                return PartialView("_List", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Detail(string UniqueID)
        {
            RequestResult result = EquipmentDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);
            ViewBag.EquipmentUniqueID = UniqueID;  //保存设备的UniqueID，当做下载时的参数使用
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
        public ActionResult Create(string OrganizationUniqueID)
        {
            try
            {
                var organization = OrganizationDataAccessor.GetOrganization(OrganizationUniqueID);

                Session["EquipmentFormAction"] = Define.EnumFormAction.Create;
                Session["EquipmentCreateFormModel"] = new CreateFormModel()
                {
                    UniqueID = Guid.NewGuid().ToString(),
                    AncestorOrganizationUniqueID = organization.AncestorOrganizationUniqueID,
                    OrganizationUniqueID = OrganizationUniqueID,
                    ParentOrganizationFullDescription = organization.FullDescription
                };

                return PartialView("_Create", Session["EquipmentCreateFormModel"]);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = EquipmentDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["EquipmentFormAction"] = Define.EnumFormAction.Create;
                Session["EquipmentCreateFormModel"] = result.Data;

                return PartialView("_Create", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model, string SpecPageStates, string MaterialPageStates, string PartPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EquipmentCreateFormModel"] as CreateFormModel;

                var specPageStateList = JsonConvert.DeserializeObject<List<string>>(SpecPageStates);

                result = EquipmentDataAccessor.SavePageState(model.SpecList, specPageStateList);

                if (result.IsSuccess)
                {
                    model.SpecList = result.Data as List<SpecModel>;

                    var materialPageStateList = JsonConvert.DeserializeObject<List<string>>(MaterialPageStates);

                    result = EquipmentDataAccessor.SavePageState(model.MaterialList, materialPageStateList);

                    if (result.IsSuccess)
                    {
                        model.MaterialList = result.Data as List<MaterialModel>;

                        var partPageStateList = JsonConvert.DeserializeObject<List<string>>(PartPageStates);

                        result = EquipmentDataAccessor.SavePageState(model.PartList, partPageStateList);

                        if (result.IsSuccess)
                        {
                            model.PartList = result.Data as List<PartModel>;

                            model.FormInput = Model.FormInput;

                            result = EquipmentDataAccessor.Create(model);

                            if (result.IsSuccess)
                            {
                                Session.Remove("EquipmentFormAction");
                                Session.Remove("EquipmentCreateFormModel");
                            }
                        }
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
            RequestResult result = EquipmentDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["EquipmentFormAction"] = Define.EnumFormAction.Edit;
                Session["EquipmentEditFormModel"] = result.Data;

                return PartialView("_Edit", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model, string SpecPageStates, string MaterialPageStates, string PartPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EquipmentEditFormModel"] as EditFormModel;

                var specPageStateList = JsonConvert.DeserializeObject<List<string>>(SpecPageStates);

                result = EquipmentDataAccessor.SavePageState(model.SpecList, specPageStateList);

                if (result.IsSuccess)
                {
                    model.SpecList = result.Data as List<SpecModel>;

                    var materialPageStateList = JsonConvert.DeserializeObject<List<string>>(MaterialPageStates);

                    result = EquipmentDataAccessor.SavePageState(model.MaterialList, materialPageStateList);

                    if (result.IsSuccess)
                    {
                        model.MaterialList = result.Data as List<MaterialModel>;

                        var partPageStateList = JsonConvert.DeserializeObject<List<string>>(PartPageStates);

                        result = EquipmentDataAccessor.SavePageState(model.PartList, partPageStateList);

                        if (result.IsSuccess)
                        {
                            model.PartList = result.Data as List<PartModel>;

                            model.FormInput = Model.FormInput;

                            result = EquipmentDataAccessor.Edit(model);

                            if (result.IsSuccess)
                            {
                                Session.Remove("EquipmentFormAction");
                                Session.Remove("EquipmentEditFormModel");
                            }
                        }
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

                result = EquipmentDataAccessor.Delete(selectedList);
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
                    result =EquipmentDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
                }
                else
                {
                     result =EquipmentDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
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

        public ActionResult GetTreeItem(string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = EquipmentDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, Session["Account"] as Account);

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

        public ActionResult InitMaintenanceOrganizationTree(string OrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = OrganizationDataAccessor.GetRootTreeItem(organizationList, OrganizationUniqueID, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_MaintenanceOrganizationTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetMaintenanceOrganizationTreeItem(string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = OrganizationDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, Session["Account"] as Account);

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
                jsonTree = string.Empty;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }

            return Content(jsonTree);
        }

        public ActionResult InitSpecSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = EquipmentSpecDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_SpecSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSpecSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string EquipmentType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = EquipmentSpecDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, EquipmentType);

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

        public ActionResult InitMaterialSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = MaterialDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_MaterialSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult InitPartMaterialSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = MaterialDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_PartMaterialSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetMaterialSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string EquipmentType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = MaterialDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, EquipmentType);

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

        public ActionResult GetSpecSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SpecSelectedList", (Session["EquipmentCreateFormModel"] as CreateFormModel).SpecList);
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SpecSelectedList", (Session["EquipmentEditFormModel"] as EditFormModel).SpecList);
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

        public ActionResult GetMaterialSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_MaterialSelectedList", (Session["EquipmentCreateFormModel"] as CreateFormModel).MaterialList);
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_MaterialSelectedList", (Session["EquipmentEditFormModel"] as EditFormModel).MaterialList);
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

        public ActionResult GetPartList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_PartList", (Session["EquipmentCreateFormModel"] as CreateFormModel).PartList);
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_PartList", (Session["EquipmentEditFormModel"] as EditFormModel).PartList);
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

        [HttpGet]
        public ActionResult CreatePart()
        {
            return PartialView("_CreatePart", new CreatePartFormModel());
        }

        [HttpPost]
        public ActionResult CreatePart(CreatePartFormModel Model, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                var uniqueID = Guid.NewGuid().ToString();
                
                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCreateFormModel"] as CreateFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        var exists = model.PartList.FirstOrDefault(x => x.Description == Model.FormInput.Description);

                        if (exists == null)
                        {
                            model.PartList.Add(new PartModel()
                            {
                                UniqueID = uniqueID,
                                Description = Model.FormInput.Description
                            });

                            Session["EquipmentCreateFormModel"] = model;

                            result.ReturnData(uniqueID);
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.EquipmentPart, Resources.Resource.Exists));
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentEditFormModel"] as EditFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        var exists = model.PartList.FirstOrDefault(x => x.Description == Model.FormInput.Description);

                        if (exists == null)
                        {
                            model.PartList.Add(new PartModel()
                            {
                                UniqueID = uniqueID,
                                Description = Model.FormInput.Description
                            });

                            Session["EquipmentEditFormModel"] = model;

                            result.ReturnData(uniqueID);
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.EquipmentPart, Resources.Resource.Exists));
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
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        [HttpGet]
        public ActionResult EditPart(string PartUniqueID)
        {
            try
            {
                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    var part = (Session["EquipmentCreateFormModel"] as CreateFormModel).PartList.First(x => x.UniqueID == PartUniqueID);

                    return PartialView("_EditPart", new EditPartFormModel()
                    {
                        UniqueID = part.UniqueID,
                        FormInput = new PartFormInput()
                        {
                            Description = part.Description
                        }
                    });
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    var part = (Session["EquipmentEditFormModel"] as EditFormModel).PartList.First(x => x.UniqueID == PartUniqueID);

                    return PartialView("_EditPart", new EditPartFormModel()
                    {
                        UniqueID = part.UniqueID,
                        FormInput = new PartFormInput()
                        {
                            Description = part.Description
                        }
                    });
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

        [HttpPost]
        public ActionResult EditPart(EditPartFormModel Model, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCreateFormModel"] as CreateFormModel;

                     result = EquipmentDataAccessor.SavePageState(model.PartList, pageStateList);

                     if (result.IsSuccess)
                     {
                         model.PartList = result.Data as List<PartModel>;

                         var exists = model.PartList.FirstOrDefault(x => x.UniqueID != Model.UniqueID && x.Description == Model.FormInput.Description);

                         if (exists == null)
                         {
                             model.PartList.First(x => x.UniqueID == Model.UniqueID).Description = Model.FormInput.Description;

                             Session["EquipmentCreateFormModel"] = model;

                             result.Success();
                         }
                         else
                         {
                             result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.EquipmentPart, Resources.Resource.Exists));
                         }
                     }
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentEditFormModel"] as EditFormModel;

                     result = EquipmentDataAccessor.SavePageState(model.PartList, pageStateList);

                     if (result.IsSuccess)
                     {
                         model.PartList = result.Data as List<PartModel>;

                         var exists = model.PartList.FirstOrDefault(x => x.UniqueID != Model.UniqueID && x.Description == Model.FormInput.Description);

                         if (exists == null)
                         {
                             model.PartList.First(x => x.UniqueID == Model.UniqueID).Description = Model.FormInput.Description;

                             Session["EquipmentEditFormModel"] = model;

                             result.Success();
                         }
                         else
                         {
                             result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.EquipmentPart, Resources.Resource.Exists));
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
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult DeletePart(string PartUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCreateFormModel"] as CreateFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        model.PartList.Remove(model.PartList.First(x => x.UniqueID == PartUniqueID));

                        Session["EquipmentCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentEditFormModel"] as EditFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        model.PartList.Remove(model.PartList.First(x => x.UniqueID == PartUniqueID));

                        Session["EquipmentEditFormModel"] = model;

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
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult AddSpec(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCreateFormModel"] as CreateFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.SpecList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.SpecList = result.Data as List<SpecModel>;

                        result = EquipmentDataAccessor.AddSpec(model.SpecList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.SpecList = result.Data as List<SpecModel>;

                            Session["EquipmentCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentEditFormModel"] as EditFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.SpecList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.SpecList = result.Data as List<SpecModel>;

                        result = EquipmentDataAccessor.AddSpec(model.SpecList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.SpecList = result.Data as List<SpecModel>;

                            Session["EquipmentEditFormModel"] = model;
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

        public ActionResult DeleteSpec(string SpecUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCreateFormModel"] as CreateFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.SpecList, pageStateList);

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

                        Session["EquipmentCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentEditFormModel"] as EditFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.SpecList, pageStateList);

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

                        Session["EquipmentEditFormModel"] = model;

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

        public ActionResult AddMaterial(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCreateFormModel"] as CreateFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.MaterialList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.MaterialList = result.Data as List<MaterialModel>;

                        result = EquipmentDataAccessor.AddMaterial(model.MaterialList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.MaterialList = result.Data as List<MaterialModel>;

                            Session["EquipmentCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentEditFormModel"] as EditFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.MaterialList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.MaterialList = result.Data as List<MaterialModel>;

                        result = EquipmentDataAccessor.AddMaterial(model.MaterialList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.MaterialList = result.Data as List<MaterialModel>;

                            Session["EquipmentEditFormModel"] = model;
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

        public ActionResult DeleteMaterial(string MaterialUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCreateFormModel"] as CreateFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.MaterialList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.MaterialList = result.Data as List<MaterialModel>;

                        model.MaterialList.Remove(model.MaterialList.First(x => x.UniqueID == MaterialUniqueID));

                        Session["EquipmentCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentEditFormModel"] as EditFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.MaterialList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.MaterialList = result.Data as List<MaterialModel>;

                        model.MaterialList.Remove(model.MaterialList.First(x => x.UniqueID == MaterialUniqueID));

                        Session["EquipmentEditFormModel"] = model;

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

        public ActionResult AddPartMaterial(string PartUniqueID, string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCreateFormModel"] as CreateFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        var part = model.PartList.First(x => x.UniqueID == PartUniqueID);

                        result = EquipmentDataAccessor.AddMaterial(part.MaterialList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            part.MaterialList = result.Data as List<MaterialModel>;

                            Session["EquipmentCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentEditFormModel"] as EditFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        var part = model.PartList.First(x => x.UniqueID == PartUniqueID);

                        result = EquipmentDataAccessor.AddMaterial(part.MaterialList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            part.MaterialList = result.Data as List<MaterialModel>;

                            Session["EquipmentEditFormModel"] = model;
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

        public ActionResult DeletePartMaterial(string PartUniqueID, string MaterialUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCreateFormModel"] as CreateFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        var part = model.PartList.First(x => x.UniqueID == PartUniqueID);

                        part.MaterialList.Remove(part.MaterialList.First(x => x.UniqueID == MaterialUniqueID));

                        Session["EquipmentCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentEditFormModel"] as EditFormModel;

                    result = EquipmentDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        var part = model.PartList.First(x => x.UniqueID == PartUniqueID);

                        part.MaterialList.Remove(part.MaterialList.First(x => x.UniqueID == MaterialUniqueID));

                        Session["EquipmentEditFormModel"] = model;

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

        public ActionResult Export(Report.EquipmentMaintenance.Models.EquipmentDetail.QueryParameters Parameters)
        {
            var model = EquipmentDetailDataAccessor.Export(Parameters) as ExcelExportModel;

            return File(model.Data, model.ContentType, model.FileName);
        }

        public ActionResult ExportEquipment(Define.EnumExcelVersion ExcelVersion)
        {
            var excel = EquipmentDataAccessor.Export(Session["QueryResults"] as GridViewModel, ExcelVersion);

            return File(excel.Data, excel.ContentType, excel.FileName);
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

                    result = EquipmentDataAccessor.UploadPhoto(UniqueID, extension);

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

        public ActionResult DeletePhoto(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(EquipmentDataAccessor.DeletePhoto(UniqueID)));
        }

#if ASE
        public ActionResult ExportQRCode(string Selecteds)
        {

            try
            {
                var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                var fileName = "QRCODE_" + Guid.NewGuid().ToString() + ".xlsx";

                var model = EquipmentDataAccessor.ExportQRCode(userList, selectedList, Session["Account"] as Account, Define.EnumExcelVersion._2007, fileName) as RequestResult;

                if (model.IsSuccess)
                {
                    var guidFileName = model.Data as string;
                    var desFileName = "設備資料.xlsx";//depart.Name + "_" + currentDate + ".xlsx";

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

#if !ASE
        public ActionResult Move(string OrganizationUniqueID, string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                result = EquipmentDataAccessor.Move(OrganizationUniqueID, selectedList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }
#endif
    }
}