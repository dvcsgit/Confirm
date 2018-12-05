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
using Models.EquipmentMaintenance.EquipmentCheckItemManagement;

#if ASE
using DataAccess.ASE;
#else
using DataAccess.EquipmentMaintenance;
using DataAccess;
#endif

using System.Web;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class EquipmentCheckItemController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = EquipmentCheckItemDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = EquipmentCheckItemDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
            var uniqueID = Guid.NewGuid().ToString();

            Session["EquipmentCheckItemFormAction"] = Define.EnumFormAction.Create;
            Session["EquipmentCheckItemCreateFormModel"] = new CreateFormModel()
            {
                UniqueID = uniqueID,
                OrganizationUniqueID = OrganizationUniqueID,
                ParentOrganizationFullDescription = OrganizationDataAccessor.GetOrganizationFullDescription(OrganizationUniqueID)
            };

            return PartialView("_Create", Session["EquipmentCheckItemCreateFormModel"]);
        }

        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = EquipmentCheckItemDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["EquipmentCheckItemFormAction"] = Define.EnumFormAction.Create;
                Session["EquipmentCheckItemCreateFormModel"] = result.Data;

                return PartialView("_Create", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model, string CheckItemPageStates, string PartPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EquipmentCheckItemCreateFormModel"] as CreateFormModel;

                var checkItemPageStateList = JsonConvert.DeserializeObject<List<string>>(CheckItemPageStates);

                result = EquipmentCheckItemDataAccessor.SavePageState(model.CheckItemList, checkItemPageStateList);

                if (result.IsSuccess)
                {
                    model.CheckItemList = result.Data as List<CheckItemModel>;

                    var partPageStateList = JsonConvert.DeserializeObject<List<string>>(PartPageStates);

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.PartList, partPageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        model.FormInput = Model.FormInput;

                        result = EquipmentCheckItemDataAccessor.Create(model);

                        if (result.IsSuccess)
                        {
                            Session.Remove("EquipmentCheckItemFormAction");
                            Session.Remove("EquipmentCheckItemCreateFormModel");
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
            RequestResult result = EquipmentCheckItemDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["EquipmentCheckItemFormAction"] = Define.EnumFormAction.Edit;
                Session["EquipmentCheckItemEditFormModel"] = result.Data;

                return PartialView("_Edit", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model, string CheckItemPageStates, string PartPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EquipmentCheckItemEditFormModel"] as EditFormModel;

                var checkItemPageStateList = JsonConvert.DeserializeObject<List<string>>(CheckItemPageStates);

                result = EquipmentCheckItemDataAccessor.SavePageState(model.CheckItemList, checkItemPageStateList);

                if (result.IsSuccess)
                {
                    model.CheckItemList = result.Data as List<CheckItemModel>;

                    var partPageStateList = JsonConvert.DeserializeObject<List<string>>(PartPageStates);

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.PartList, partPageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        model.FormInput = Model.FormInput;

                        result = EquipmentCheckItemDataAccessor.Edit(model);

                        if (result.IsSuccess)
                        {
                            Session.Remove("EquipmentCheckItemFormAction");
                            Session.Remove("EquipmentCheckItemEditFormModel");
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

        public ActionResult InitCheckItemSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_CheckItemSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult InitPartCheckItemSelectTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_PartCheckItemSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetCheckItemSelectTreeItem(string RefOrganizationUniqueID, string OrganizationUniqueID, string CheckType)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = CheckItemDataAccessor.GetTreeItem(organizationList, RefOrganizationUniqueID, OrganizationUniqueID, CheckType);

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

        public ActionResult GetCheckItemSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_CheckItemSelectedList", (Session["EquipmentCheckItemCreateFormModel"] as CreateFormModel).CheckItemList);
                }
                else if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_CheckItemSelectedList", (Session["EquipmentCheckItemEditFormModel"] as EditFormModel).CheckItemList);
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
                if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_PartList", (Session["EquipmentCheckItemCreateFormModel"] as CreateFormModel).PartList);
                }
                else if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_PartList", (Session["EquipmentCheckItemEditFormModel"] as EditFormModel).PartList);
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

                if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCheckItemCreateFormModel"] as CreateFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        var exists = model.PartList.FirstOrDefault(x => x.Description == Model.FormInput.Description);

                        if (exists == null)
                        {
                            model.PartList.Add(new PartModel()
                            {
                                UniqueID = uniqueID,
                                Description = Model.FormInput.Description
                            });

                            Session["EquipmentCheckItemCreateFormModel"] = model;

                            result.ReturnData(uniqueID);
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.EquipmentPart, Resources.Resource.Exists));
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentCheckItemEditFormModel"] as EditFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        var exists = model.PartList.FirstOrDefault(x => x.Description == Model.FormInput.Description);

                        if (exists == null)
                        {
                            model.PartList.Add(new PartModel()
                            {
                                UniqueID = uniqueID,
                                Description = Model.FormInput.Description
                            });

                            Session["EquipmentCheckItemEditFormModel"] = model;

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
                if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    var part = (Session["EquipmentCheckItemCreateFormModel"] as CreateFormModel).PartList.First(x => x.UniqueID == PartUniqueID);

                    return PartialView("_EditPart", new EditPartFormModel()
                    {
                        UniqueID = part.UniqueID,
                        FormInput = new PartFormInput()
                        {
                            Description = part.Description
                        }
                    });
                }
                else if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    var part = (Session["EquipmentCheckItemEditFormModel"] as EditFormModel).PartList.First(x => x.UniqueID == PartUniqueID);

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

                if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCheckItemCreateFormModel"] as CreateFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        var exists = model.PartList.FirstOrDefault(x => x.UniqueID != Model.UniqueID && x.Description == Model.FormInput.Description);

                        if (exists == null)
                        {
                            model.PartList.First(x => x.UniqueID == Model.UniqueID).Description = Model.FormInput.Description;

                            Session["EquipmentCheckItemCreateFormModel"] = model;

                            result.Success();
                        }
                        else
                        {
                            result.ReturnFailedMessage(string.Format("{0} {1}", Resources.Resource.EquipmentPart, Resources.Resource.Exists));
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentCheckItemEditFormModel"] as EditFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        var exists = model.PartList.FirstOrDefault(x => x.UniqueID != Model.UniqueID && x.Description == Model.FormInput.Description);

                        if (exists == null)
                        {
                            model.PartList.First(x => x.UniqueID == Model.UniqueID).Description = Model.FormInput.Description;

                            Session["EquipmentCheckItemEditFormModel"] = model;

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

                if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCheckItemCreateFormModel"] as CreateFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        model.PartList.Remove(model.PartList.First(x => x.UniqueID == PartUniqueID));

                        Session["EquipmentCheckItemCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentCheckItemEditFormModel"] as EditFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        model.PartList.Remove(model.PartList.First(x => x.UniqueID == PartUniqueID));

                        Session["EquipmentCheckItemEditFormModel"] = model;

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

        public ActionResult AddCheckItem(string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCheckItemCreateFormModel"] as CreateFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.CheckItemList = result.Data as List<CheckItemModel>;

                        result = EquipmentCheckItemDataAccessor.AddCheckItem(model.CheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.CheckItemList = result.Data as List<CheckItemModel>;

                            Session["EquipmentCheckItemCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentCheckItemEditFormModel"] as EditFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.CheckItemList = result.Data as List<CheckItemModel>;

                        result = EquipmentCheckItemDataAccessor.AddCheckItem(model.CheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            model.CheckItemList = result.Data as List<CheckItemModel>;

                            Session["EquipmentCheckItemEditFormModel"] = model;
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

        public ActionResult DeleteCheckItem(string CheckItemUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCheckItemCreateFormModel"] as CreateFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.CheckItemList = result.Data as List<CheckItemModel>;

                        model.CheckItemList.Remove(model.CheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["EquipmentCheckItemCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentCheckItemEditFormModel"] as EditFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.CheckItemList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.CheckItemList = result.Data as List<CheckItemModel>;

                        model.CheckItemList.Remove(model.CheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["EquipmentCheckItemEditFormModel"] = model;

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

        public ActionResult AddPartCheckItem(string PartUniqueID, string Selecteds, string PageStates, string RefOrganizationUniqueID)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCheckItemCreateFormModel"] as CreateFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        var part = model.PartList.First(x => x.UniqueID == PartUniqueID);

                        result = EquipmentCheckItemDataAccessor.AddCheckItem(part.CheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            part.CheckItemList = result.Data as List<CheckItemModel>;

                            Session["EquipmentCheckItemCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentCheckItemEditFormModel"] as EditFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        var part = model.PartList.First(x => x.UniqueID == PartUniqueID);

                        result = EquipmentCheckItemDataAccessor.AddCheckItem(part.CheckItemList, selectedList, RefOrganizationUniqueID);

                        if (result.IsSuccess)
                        {
                            part.CheckItemList = result.Data as List<CheckItemModel>;

                            Session["EquipmentCheckItemEditFormModel"] = model;
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

        public ActionResult DeletePartCheckItem(string PartUniqueID, string CheckItemUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["EquipmentCheckItemCreateFormModel"] as CreateFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        var part = model.PartList.First(x => x.UniqueID == PartUniqueID);

                        part.CheckItemList.Remove(part.CheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["EquipmentCheckItemCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["EquipmentCheckItemFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["EquipmentCheckItemEditFormModel"] as EditFormModel;

                    result = EquipmentCheckItemDataAccessor.SavePageState(model.PartList, pageStateList);

                    if (result.IsSuccess)
                    {
                        model.PartList = result.Data as List<PartModel>;

                        var part = model.PartList.First(x => x.UniqueID == PartUniqueID);

                        part.CheckItemList.Remove(part.CheckItemList.First(x => x.UniqueID == CheckItemUniqueID));

                        Session["EquipmentCheckItemEditFormModel"] = model;

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
    }
}