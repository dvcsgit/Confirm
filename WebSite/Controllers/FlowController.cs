using System;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.FlowManagement;

#if ASE
using DataAccess.ASE;
#else
using DataAccess;
#endif
using System.Web;

namespace WebSite.Controllers
{
    public class FlowController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Query(QueryFormModel Model)
        {
            RequestResult result = FlowDataAccessor.Query(Model.Parameters, Session["Account"] as Account);

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
            RequestResult result = FlowDataAccessor.GetDetailViewModel(UniqueID, Session["Account"] as Account);

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
            RequestResult result = FlowDataAccessor.GetCreateFormModel(OrganizationUniqueID);

            if (result.IsSuccess)
            {
                Session["FlowFormAction"] = Define.EnumFormAction.Create;
                Session["FlowCreateFormModel"] = result.Data;

                return PartialView("_Create", Session["FlowCreateFormModel"]);
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
                var model = Session["FlowCreateFormModel"] as CreateFormModel;

                var pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                result = FlowDataAccessor.SavePageState(pageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.VerifyOrganizationList = result.Data as List<VerifyOrganizationModel>;

                    result = FlowDataAccessor.Create(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("FlowFormAction");
                        Session.Remove("FlowCreateFormModel");
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
        public ActionResult Copy(string UniqueID)
        {
            RequestResult result = FlowDataAccessor.GetCopyFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["FlowFormAction"] = Define.EnumFormAction.Create;
                Session["FlowCreateFormModel"] = result.Data;

                return PartialView("_Create", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = FlowDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["FlowFormAction"] = Define.EnumFormAction.Edit;
                Session["FlowEditFormModel"] = result.Data;

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
                var model = Session["FlowEditFormModel"] as EditFormModel;

                var pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                result = FlowDataAccessor.SavePageState(pageStateList);

                if (result.IsSuccess)
                {
                    model.FormInput = Model.FormInput;
                    model.VerifyOrganizationList = result.Data as List<VerifyOrganizationModel>;

                    result = FlowDataAccessor.Edit(model);

                    if (result.IsSuccess)
                    {
                        Session.Remove("FlowFormAction");
                        Session.Remove("FlowEditFormModel");
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
                result = FlowDataAccessor.Delete(JsonConvert.DeserializeObject<List<string>>(Selecteds));
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
                var account = Session["Account"] as Account;

                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = new RequestResult();

                if (account.UserRootOrganizationUniqueID == "*")
                {
                    result = FlowDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
                }
                else
                {
                    result = FlowDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
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

                RequestResult result = FlowDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, Session["Account"] as Account);

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

        public ActionResult InitSelectTree(string AncestorOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = OrganizationDataAccessor.GetRootTreeItem(organizationList, AncestorOrganizationUniqueID, Session["Account"] as Account);

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

        public ActionResult GetSelectTreeItem(string OrganizationUniqueID)
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
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                jsonTree = string.Empty;
            }

            return Content(jsonTree);
        }

        public ActionResult GetSelectedList()
        {
            try
            {
                if ((Define.EnumFormAction)Session["FlowFormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_SelectedList", (Session["FlowCreateFormModel"] as CreateFormModel).VerifyOrganizationList);
                }
                else if ((Define.EnumFormAction)Session["FlowFormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_SelectedList", (Session["FlowEditFormModel"] as EditFormModel).VerifyOrganizationList);
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

        public ActionResult AddSelect(string Selecteds, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FlowFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FlowCreateFormModel"] as CreateFormModel;

                    result = FlowDataAccessor.SavePageState(pageStateList);

                    if (result.IsSuccess)
                    {
                        model.VerifyOrganizationList = result.Data as List<VerifyOrganizationModel>;

                        result = FlowDataAccessor.AddOrganization(model.VerifyOrganizationList, selectedList);

                        if (result.IsSuccess)
                        {
                            model.VerifyOrganizationList = result.Data as List<VerifyOrganizationModel>;

                            Session["FlowCreateFormModel"] = model;
                        }
                    }
                }
                else if ((Define.EnumFormAction)Session["FlowFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FlowEditFormModel"] as EditFormModel;

                    result = FlowDataAccessor.SavePageState(pageStateList);

                    if (result.IsSuccess)
                    {
                        model.VerifyOrganizationList = result.Data as List<VerifyOrganizationModel>;

                        result = FlowDataAccessor.AddOrganization(model.VerifyOrganizationList, selectedList);

                        if (result.IsSuccess)
                        {
                            model.VerifyOrganizationList = result.Data as List<VerifyOrganizationModel>;

                            Session["FlowEditFormModel"] = model;
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

        public ActionResult DeleteSelected(string OrganizationUniqueID, string PageStates)
        {
            RequestResult result = new RequestResult();

            try
            {
                List<string> pageStateList = JsonConvert.DeserializeObject<List<string>>(PageStates);

                if ((Define.EnumFormAction)Session["FlowFormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FlowCreateFormModel"] as CreateFormModel;

                    result = FlowDataAccessor.SavePageState(pageStateList);

                    if (result.IsSuccess)
                    {
                        model.VerifyOrganizationList = result.Data as List<VerifyOrganizationModel>;

                        model.VerifyOrganizationList.Remove(model.VerifyOrganizationList.First(x => x.UniqueID == OrganizationUniqueID));

                        model.VerifyOrganizationList = model.VerifyOrganizationList.OrderBy(x => x.Seq).ToList();

                        int seq = 1;

                        foreach (var verifyOrganization in model.VerifyOrganizationList)
                        {
                            verifyOrganization.Seq = seq;

                            seq++;
                        }

                        Session["FlowCreateFormModel"] = model;

                        result.Success();
                    }
                }
                else if ((Define.EnumFormAction)Session["FlowFormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FlowEditFormModel"] as EditFormModel;

                    result = FlowDataAccessor.SavePageState(pageStateList);

                    if (result.IsSuccess)
                    {
                        model.VerifyOrganizationList = result.Data as List<VerifyOrganizationModel>;

                        model.VerifyOrganizationList.Remove(model.VerifyOrganizationList.First(x => x.UniqueID == OrganizationUniqueID));

                        model.VerifyOrganizationList = model.VerifyOrganizationList.OrderBy(x => x.Seq).ToList();

                        int seq = 1;

                        foreach (var verifyOrganization in model.VerifyOrganizationList)
                        {
                            verifyOrganization.Seq = seq;

                            seq++;
                        }

                        Session["FlowEditFormModel"] = model;

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
