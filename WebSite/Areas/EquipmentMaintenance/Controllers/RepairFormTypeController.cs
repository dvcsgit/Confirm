using System.Web.Mvc;
using Newtonsoft.Json;
using Utility.Models;
using Models.EquipmentMaintenance.RepairFormTypeManagement;
using Models.Authenticated;
using Models.Shared;
using System.Collections.Generic;
using System;
using System.Reflection;
using Utility;
using System.Linq;

#if ASE
using DataAccess.ASE;
#else
using DataAccess;
using DataAccess.EquipmentMaintenance;
#endif

using System.Web;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class RepairFormTypeController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = RepairFormTypeDataAccessor.Query(Parameters);

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
            RequestResult result = RepairFormTypeDataAccessor.GetDetailViewModel(UniqueID);

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
        public ActionResult Create(string AncestorOrganizationUniqueID)
        {
            try
            {
                Session["RepairFormTypeFormAction"] = Define.EnumFormAction.Create;
                Session["RepairFormTypeCreateFormModel"] = new CreateFormModel()
                {
                    AncestorOrganizationUniqueID = AncestorOrganizationUniqueID,
                    AncestorOrganizationDescription = OrganizationDataAccessor.GetOrganizationDescription(AncestorOrganizationUniqueID)
                };

                return PartialView("_Create", Session["RepairFormTypeCreateFormModel"]);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        [HttpGet]
        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = RepairFormTypeDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["RepairFormTypeFormAction"] = Define.EnumFormAction.Create;
                Session["RepairFormTypeCreateFormModel"] = result.Data as CreateFormModel;

                return PartialView("_Create", Session["RepairFormTypeCreateFormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model, string SubjectPageStates, string ColumnPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["RepairFormTypeCreateFormModel"] as CreateFormModel;

                var subjectPageStateList = JsonConvert.DeserializeObject<List<string>>(SubjectPageStates);

                int seq = 1;

                foreach (var subjectPageState in subjectPageStateList)
                {
                    model.SubjectList.First(x => x.UniqueID == subjectPageState).Seq = seq;

                    seq++;
                }

                var columnPageStateList = JsonConvert.DeserializeObject<List<string>>(ColumnPageStates);

                seq = 1;

                foreach (var columnPageState in columnPageStateList)
                {
                    model.ColumnList.First(x => x.UniqueID == columnPageState).Seq = seq;

                    seq++;
                }

                model.FormInput = Model.FormInput;

                result = RepairFormTypeDataAccessor.Create(model);
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
            RequestResult result = RepairFormTypeDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["RepairFormTypeFormAction"] = Define.EnumFormAction.Edit;
                Session["RepairFormTypeEditFormModel"] = result.Data as EditFormModel;

                return PartialView("_Edit", Session["RepairFormTypeEditFormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model, string SubjectPageStates, string ColumnPageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["RepairFormTypeEditFormModel"] as EditFormModel;

                var subjectPageStateList = JsonConvert.DeserializeObject<List<string>>(SubjectPageStates);

                int seq = 1;

                foreach (var subjectPageState in subjectPageStateList)
                {
                    model.SubjectList.First(x => x.UniqueID == subjectPageState).Seq = seq;

                    seq++;
                }

                var columnPageStateList = JsonConvert.DeserializeObject<List<string>>(ColumnPageStates);

                seq = 1;

                foreach (var columnPageState in columnPageStateList)
                {
                    model.ColumnList.First(x => x.UniqueID == columnPageState).Seq = seq;

                    seq++;
                }

                model.FormInput = Model.FormInput;

                result = RepairFormTypeDataAccessor.Edit(model);
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

                result = RepairFormTypeDataAccessor.Delete(selectedList);
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

                RequestResult result = RepairFormTypeDataAccessor.GetTreeItem(organizationList, "*", Session["Account"] as Account);

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

        public ActionResult GetTreeItem(string AncestorOrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = RepairFormTypeDataAccessor.GetTreeItem(organizationList, AncestorOrganizationUniqueID, Session["Account"] as Account);

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

        public ActionResult InitSubjectSelectTree(string AncestorOrganizationUniqueID)
        {
            try
            {
                RequestResult result = RepairFormTypeDataAccessor.GetSubjectTreeItem(AncestorOrganizationUniqueID);

                if (result.IsSuccess)
                {
                    return PartialView("_SubjectSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult InitColumnSelectTree(string AncestorOrganizationUniqueID)
        {
            try
            {
                RequestResult result = RepairFormTypeDataAccessor.GetColumnTreeItem(AncestorOrganizationUniqueID);

                if (result.IsSuccess)
                {
                    return PartialView("_ColumnSelectTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetSubjectSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["RepairFormTypeFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SubjectSelectedList", (Session["RepairFormTypeCreateFormModel"] as CreateFormModel).SubjectList);
                }
                else if ((Define.EnumFormAction)Session["RepairFormTypeFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SubjectSelectedList", (Session["RepairFormTypeEditFormModel"] as EditFormModel).SubjectList);
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

        public ActionResult GetColumnSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["RepairFormTypeFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_ColumnSelectedList", (Session["RepairFormTypeCreateFormModel"] as CreateFormModel).ColumnList);
                }
                else if ((Define.EnumFormAction)Session["RepairFormTypeFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_ColumnSelectedList", (Session["RepairFormTypeEditFormModel"] as EditFormModel).ColumnList);
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

        public ActionResult AddSubject(string Selecteds, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["RepairFormTypeFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RepairFormTypeCreateFormModel"] as CreateFormModel;

                    int seq = 1;

                    foreach (var pageState in pageStateList)
                    {
                        model.SubjectList.First(x => x.UniqueID == pageState).Seq = seq;

                        seq++;
                    }

                    result = RepairFormTypeDataAccessor.AddSubject(model.SubjectList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.SubjectList = result.Data as List<SubjectModel>;

                        Session["RepairFormTypeCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["RepairFormTypeFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RepairFormTypeEditFormModel"] as EditFormModel;

                    int seq = 1;

                    foreach (var pageState in pageStateList)
                    {
                        model.SubjectList.First(x => x.UniqueID == pageState).Seq = seq;

                        seq++;
                    }

                    result = RepairFormTypeDataAccessor.AddSubject(model.SubjectList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.SubjectList = result.Data as List<SubjectModel>;

                        Session["RepairFormTypeEditFormModel"] = model;
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

        public ActionResult DeleteSubject(string SubjectUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["RepairFormTypeFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RepairFormTypeCreateFormModel"] as CreateFormModel;

                    int seq = 1;

                    foreach (var pageState in pageStateList)
                    {
                        model.SubjectList.First(x => x.UniqueID == pageState).Seq = seq;

                        seq++;
                    }

                    model.SubjectList.Remove(model.SubjectList.First(x => x.UniqueID == SubjectUniqueID));

                    model.SubjectList = model.SubjectList.OrderBy(x => x.Seq).ToList();

                    seq = 1;

                    foreach (var subject in model.SubjectList)
                    {
                        subject.Seq = seq;

                        seq++;
                    }

                    Session["RepairFormTypeCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["RepairFormTypeFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RepairFormTypeEditFormModel"] as EditFormModel;

                    int seq = 1;

                    foreach (var pageState in pageStateList)
                    {
                        model.SubjectList.First(x => x.UniqueID == pageState).Seq = seq;

                        seq++;
                    }

                    model.SubjectList.Remove(model.SubjectList.First(x => x.UniqueID == SubjectUniqueID));

                    model.SubjectList = model.SubjectList.OrderBy(x => x.Seq).ToList();

                    seq = 1;

                    foreach (var subject in model.SubjectList)
                    {
                        subject.Seq = seq;

                        seq++;
                    }

                    Session["RepairFormTypeEditFormModel"] = model;

                    result.Success();
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

        public ActionResult AddColumn(string Selecteds, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["RepairFormTypeFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RepairFormTypeCreateFormModel"] as CreateFormModel;

                    int seq = 1;

                    foreach (var pageState in pageStateList)
                    {
                        model.ColumnList.First(x => x.UniqueID == pageState).Seq = seq;

                        seq++;
                    }

                    result = RepairFormTypeDataAccessor.AddColumn(model.ColumnList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.ColumnList = result.Data as List<ColumnModel>;

                        Session["RepairFormTypeCreateFormModel"] = model;
                    }
                }
                else if ((Define.EnumFormAction)Session["RepairFormTypeFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RepairFormTypeEditFormModel"] as EditFormModel;

                    int seq = 1;

                    foreach (var pageState in pageStateList)
                    {
                        model.ColumnList.First(x => x.UniqueID == pageState).Seq = seq;

                        seq++;
                    }

                    result = RepairFormTypeDataAccessor.AddColumn(model.ColumnList, selectedList);

                    if (result.IsSuccess)
                    {
                        model.ColumnList = result.Data as List<ColumnModel>;

                        Session["RepairFormTypeEditFormModel"] = model;
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

        public ActionResult DeleteColumn(string ColumnUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["RepairFormTypeFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["RepairFormTypeCreateFormModel"] as CreateFormModel;

                    int seq = 1;

                    foreach (var pageState in pageStateList)
                    {
                        model.ColumnList.First(x => x.UniqueID == pageState).Seq = seq;

                        seq++;
                    }

                    model.ColumnList.Remove(model.ColumnList.First(x => x.UniqueID == ColumnUniqueID));

                    model.ColumnList = model.ColumnList.OrderBy(x => x.Seq).ToList();

                    seq = 1;

                    foreach (var column in model.ColumnList)
                    {
                        column.Seq = seq;

                        seq++;
                    }

                    Session["RepairFormTypeCreateFormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["RepairFormTypeFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["RepairFormTypeEditFormModel"] as EditFormModel;

                    int seq = 1;

                    foreach (var pageState in pageStateList)
                    {
                        model.ColumnList.First(x => x.UniqueID == pageState).Seq = seq;

                        seq++;
                    }

                    model.ColumnList.Remove(model.ColumnList.First(x => x.UniqueID == ColumnUniqueID));

                    model.ColumnList = model.ColumnList.OrderBy(x => x.Seq).ToList();

                    seq = 1;

                    foreach (var column in model.ColumnList)
                    {
                        column.Seq = seq;

                        seq++;
                    }

                    Session["RepairFormTypeEditFormModel"] = model;

                    result.Success();
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
