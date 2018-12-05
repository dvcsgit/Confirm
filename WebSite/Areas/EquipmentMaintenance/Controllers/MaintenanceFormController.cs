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
using Models.EquipmentMaintenance.MaintenanceFormManagement;
using Webdiyer.WebControls.Mvc;

#if ASE
using DataAccess.ASE;
#else
using DataAccess;
using DataAccess.EquipmentMaintenance;
#endif

using System.Web;
using System.ComponentModel;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class MaintenanceFormController : Controller
    {
        #region Reviewed
        public ActionResult Index(string Status, string VHNO)
        {
            if (!string.IsNullOrEmpty(Status))
            {
                var model = new QueryFormModel();

                foreach (var item in model.StatusSelectItemList)
                {
                    if (item.Value == Status)
                    {
                        item.Selected = true;
                    }
                    else
                    {
                        item.Selected = false;
                    }
                }

                return View(model);
            }
            else
            {
                return View(new QueryFormModel()
                {
                    Parameters = new QueryParameters()
                    {
                        VHNO = VHNO
                    }
                });
            }
        }

        public ActionResult Query(QueryFormModel Model)
        {
            var accountList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = MaintenanceFormDataAccessor.Query(Model.Parameters, accountList, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["QueryResults"] = result.Data;

                return PartialView("_List", Session["QueryResults"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Export(Define.EnumExcelVersion ExcelVersion)
        {
            var accountList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            //var model = MaintenanceFormDataAccessor.Export(Session["QueryResults"] as GridViewModel, ExcelVersion).Data as ExcelExportModel;

            var model = MaintenanceFormDataAccessor.Export(accountList, Session["QueryResults"] as GridViewModel, ExcelVersion).Data as ExcelExportModel;

            return File(model.Data, model.ContentType, model.FileName);
        }

        public ActionResult ExportForm(Define.EnumExcelVersion ExcelVersion)
        {
            var model = MaintenanceFormDataAccessor.ExportForm(Session["QueryResults"] as GridViewModel, ExcelVersion).Data as ExcelExportModel;

            return File(model.Data, model.ContentType, model.FileName);
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

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            var accountList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = MaintenanceFormDataAccessor.GetEditFormModel(UniqueID, accountList);

            if (result.IsSuccess)
            {
                Session["EditFormModel"] = result.Data;

                return PartialView("_Edit", Session["EditFormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit()
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EditFormModel"] as EditFormModel;

                result = MaintenanceFormDataAccessor.Edit(model);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult Approve(string UniqueID, string Remark)
        {
            return Content(JsonConvert.SerializeObject(MaintenanceFormDataAccessor.Approve(UniqueID, Remark, Session["Account"] as Account)));
        }

        public ActionResult Reject(string UniqueID, string Remark)
        {
            return Content(JsonConvert.SerializeObject(MaintenanceFormDataAccessor.Reject(UniqueID, Remark, Session["Account"] as Account)));
        }

        public ActionResult ExtendApprove(string UniqueID, string Remark)
        {
            return Content(JsonConvert.SerializeObject(MaintenanceFormDataAccessor.ExtendApprove(UniqueID, Remark, Session["Account"] as Account)));
        }

        public ActionResult ExtendReject(string UniqueID, string Remark)
        {
            return Content(JsonConvert.SerializeObject(MaintenanceFormDataAccessor.ExtendReject(UniqueID, Remark, Session["Account"] as Account)));
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
                    result = OrganizationDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
                }
                else
                {
                    result = OrganizationDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
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

        public ActionResult Extend(string UniqueID)
        {
            RequestResult result = MaintenanceFormDataAccessor.GetExtendFormModel(UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_Extend", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Extend(ExtendFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(MaintenanceFormDataAccessor.Extend(Model)));
        }

        public ActionResult TakeJob(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                result = MaintenanceFormDataAccessor.TakeJob(selectedList, Session["Account"] as Account);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult GetFileList()
        {
            try
            {
                return PartialView("_FileList", (Session["EditFormModel"] as EditFormModel).FormViewModel.FileList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        [HttpGet]
        public ActionResult FileUpload()
        {
            return PartialView("_FileUpload");
        }

        [HttpPost]
        public ActionResult Upload()
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    var model = Session["EditFormModel"] as EditFormModel;

                    int seq = 1;

                    if (model.FormViewModel.FileList.Count > 0)
                    {
                        seq = model.FormViewModel.FileList.Max(x => x.Seq) + 1;
                    }

                    var fileName = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('\\') + 1);

                    var fileModel = new FileModel()
                    {
                        TempUniqueID = Guid.NewGuid().ToString(),
                        Seq = seq,
                        FileName = fileName.Substring(0, fileName.LastIndexOf('.')),
                        Extension = fileName.Substring(fileName.LastIndexOf('.') + 1),
                        UploadTime = DateTime.Now,
                        Size = Request.Files[0].ContentLength,
                        IsSaved = false
                    };

                    Request.Files[0].SaveAs(fileModel.TempFullFileName);

                    model.FormViewModel.FileList.Add(fileModel);

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

        public ActionResult DeleteFile(int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EditFormModel"] as EditFormModel;

                model.FormViewModel.FileList.Remove(model.FormViewModel.FileList.First(x => x.Seq == Seq));

                Session["EditFormModel"] = model;

                result.Success();
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult GetWorkingHourList()
        {
            try
            {
                return PartialView("_WorkingHourList", (Session["EditFormModel"] as EditFormModel).FormViewModel.WorkingHourList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        [HttpGet]
        public ActionResult CreateWorkingHour()
        {
            return PartialView("_CreateWorkingHour", new CreateWorkingHourFormModel());
        }


        [HttpPost]
        public ActionResult CreateWorkingHour(CreateWorkingHourFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EditFormModel"] as EditFormModel;

                result = MaintenanceFormDataAccessor.CreateWorkingHour(model.FormViewModel.WorkingHourList, Model);

                if (result.IsSuccess)
                {
                    model.FormViewModel.WorkingHourList = result.Data as List<WorkingHourModel>;

                    Session["EditFormModel"] = model;
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

        public ActionResult DeleteWorkingHour(int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EditFormModel"] as EditFormModel;

                model.FormViewModel.WorkingHourList.Remove(model.FormViewModel.WorkingHourList.First(x => x.Seq == Seq));

                Session["EditFormModel"] = model;

                result.Success();
            }
            catch (Exception ex)
            {
                Error err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        //public ActionResult GetResultList()
        //{
        //    try
        //    {
        //        return PartialView("_ResultList", (Session["EditFormModel"] as EditFormModel).FormViewModel.ResultList);
        //    }
        //    catch (Exception ex)
        //    {
        //        var err = new Error(MethodBase.GetCurrentMethod(), ex);

        //        Logger.Log(err);

        //        return PartialView("_Error", err);
        //    }
        //}

        public ActionResult GetStandardList()
        {
            try
            {
                return PartialView("_StandardList", (Session["EditFormModel"] as EditFormModel).FormViewModel.StandardList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        [HttpGet]
        public ActionResult CreateResult()
        {
            try
            {
                var model = Session["EditFormModel"] as EditFormModel;

                return PartialView("_CreateResult", new CreateResultFormModel
                {
                    StandardList = model.FormViewModel.StandardList
                });
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        [HttpPost]
        public ActionResult CreateResult(string Remark, string StandardResults)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EditFormModel"] as EditFormModel;

                var standardResultList = JsonConvert.DeserializeObject<List<string>>(StandardResults);

                result = MaintenanceFormDataAccessor.CreateResult(model, Remark, standardResultList, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    //model.FormViewModel.ResultList = result.Data as List<ResultModel>;
                    model.FormViewModel.StandardList = result.Data as List<StandardModel>;

                    Session["EditFormModel"] = model;
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

        public ActionResult GetUserOptions([DefaultValue(1)]int PageIndex, [DefaultValue(10)]int PageSize, string Term, [DefaultValue(false)]bool IsInit)
        {
            var accountList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = MaintenanceFormDataAccessor.GetUserOptions(accountList, Term, IsInit);

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

        public ActionResult Download(string MFormUniqueID, int Seq)
        {
            var file = MaintenanceFormDataAccessor.GetFile(MFormUniqueID, Seq);

            return File(file.FullFileName, file.ContentType, file.Display);
        }
        #endregion
        

        

       

        

       

       

        public ActionResult Submit()
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["EditFormModel"] as EditFormModel;

                result = MaintenanceFormDataAccessor.Edit(model);

                if (result.IsSuccess)
                {
                    result = MaintenanceFormDataAccessor.Submit(model.UniqueID);
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

        
    }
}