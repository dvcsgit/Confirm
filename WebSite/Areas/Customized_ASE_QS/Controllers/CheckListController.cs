#if ASE
using DataAccess.ASE;
using DataAccess.ASE.QS;
using Models.ASE.QS.CheckListManagement;
using Models.Authenticated;
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

namespace WebSite.Areas.Customized_ASE_QS.Controllers
{
    public class CheckListController : Controller
    {
        public ActionResult Index()
        {
            RequestResult result = CheckListDataAccessor.GetQueryFormModel();

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

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = CheckListDataAccessor.Query(Parameters);

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
            RequestResult result = CheckListDataAccessor.GetDetailViewModel(UniqueID);

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
        public ActionResult Create(string FactoryUniqueID)
        {
            RequestResult result = CheckListDataAccessor.GetCreateFormModel(FactoryUniqueID, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["FormAction"] = Define.EnumFormAction.Create;
                Session["FormModel"] = result.Data;

                return PartialView("_Create", Session["FormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["FormModel"] as CreateFormModel;

                model.FormInput = Model.FormInput;

                result = CheckListDataAccessor.Create(model, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    Session.Remove("FormAction");
                    Session.Remove("FormModel");
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
            RequestResult result = CheckListDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                Session["FormAction"] = Define.EnumFormAction.Edit;
                Session["FormModel"] = result.Data;

                return PartialView("_Edit", Session["FormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["FormModel"] as EditFormModel;

                model.FormInput = Model.FormInput;

                result = CheckListDataAccessor.Edit(model, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    Session.Remove("FormAction");
                    Session.Remove("FormModel");
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
            RequestResult result = CheckListDataAccessor.GetCopyFormModel(UniqueID, Session["Account"] as Account);

            if (result.IsSuccess)
            {
                Session["FormAction"] = Define.EnumFormAction.Create;
                Session["FormModel"] = result.Data;

                return PartialView("_Create", Session["FormModel"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Delete(string UniqueID)
        {
            return Content(JsonConvert.SerializeObject(CheckListDataAccessor.Delete(UniqueID)));
        }

        public ActionResult GetUserOptions([DefaultValue(1)]int PageIndex, [DefaultValue(10)]int PageSize, string Term, [DefaultValue(false)]bool IsInit)
        {
            var userList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.UserModel>>("Users", () => AccountDataAccessor.GetAllUser());

            RequestResult result = CheckListDataAccessor.GetUserOptions(userList, Term, IsInit);

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

        public ActionResult GetPhotoList(string CheckItemUniqueID, int CheckItemSeq)
        {
            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    return PartialView("_PhotoList", (Session["FormModel"] as CreateFormModel).PhotoList.Where(x => x.CheckItemUniqueID == CheckItemUniqueID && x.CheckItemSeq == CheckItemSeq).ToList());
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    return PartialView("_PhotoList", (Session["FormModel"] as EditFormModel).PhotoList.Where(x => x.CheckItemUniqueID == CheckItemUniqueID && x.CheckItemSeq == CheckItemSeq).ToList());
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

        public ActionResult DeletePhoto(string CheckItemUniqueID, int CheckItemSeq, int Seq)
        {
            RequestResult result = new RequestResult();

            try
            {
                if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                {
                    var model = Session["FormModel"] as CreateFormModel;

                    var checkItem = new CheckItemModel();

                    foreach (var checkType in model.CheckTypeList)
                    {
                        checkItem = checkType.CheckItemList.FirstOrDefault(x => x.UniqueID == CheckItemUniqueID);

                        if (checkItem != null)
                        {
                            break;
                        }
                    }

                    var checkResult = checkItem.CheckResultList.First(x => x.Seq == CheckItemSeq);

                    checkResult.PhotoList.Remove(checkResult.PhotoList.First(x => x.Seq == Seq));

                    Session["FormModel"] = model;

                    result.Success();
                }
                else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                {
                    var model = Session["FormModel"] as EditFormModel;

                    var checkItem = new CheckItemModel();

                    foreach (var checkType in model.CheckTypeList)
                    {
                        checkItem = checkType.CheckItemList.FirstOrDefault(x => x.UniqueID == CheckItemUniqueID);

                        if (checkItem != null)
                        {
                            break;
                        }
                    }

                    var checkResult = checkItem.CheckResultList.First(x => x.Seq == CheckItemSeq);

                    checkResult.PhotoList.Remove(checkResult.PhotoList.First(x => x.Seq == Seq));

                    result.Success();
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
        public ActionResult PhotoUpload(string CheckItemUniqueID, int CheckItemSeq)
        {
            return PartialView("_PhotoUpload", string.Format("{0}_{1}", CheckItemUniqueID, CheckItemSeq));
        }

        [HttpPost]
        public ActionResult UploadPhoto(string CheckItemUniqueID, int CheckItemSeq)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Request.Files != null && Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
                {
                    if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Create)
                    {
                        var model = Session["FormModel"] as CreateFormModel;

                        var checkItem = new CheckItemModel();

                        foreach (var checkType in model.CheckTypeList)
                        {
                            checkItem = checkType.CheckItemList.FirstOrDefault(x => x.UniqueID == CheckItemUniqueID);

                            if (checkItem != null)
                            {
                                break;
                            }
                        }

                        var checkResult = checkItem.CheckResultList.FirstOrDefault(x => x.Seq == CheckItemSeq);

                        if (checkResult == null)
                        {
                            checkResult = new CheckResultModel()
                            {
                                Seq = CheckItemSeq
                            };

                            checkItem.CheckResultList.Add(checkResult);
                        }

                        int seq = 1;

                        if (checkResult.PhotoList.Count > 0)
                        {
                            seq = model.PhotoList.Max(x => x.Seq) + 1;
                        }

                        var photoName = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('\\') + 1);

                        var photoModel = new PhotoModel()
                        {
                            TempUniqueID = Guid.NewGuid().ToString(),
                            Seq = seq,
                            FormUniqueID = "",
                            CheckItemUniqueID = CheckItemUniqueID,
                            CheckItemSeq = CheckItemSeq,
                            Extension = photoName.Substring(photoName.LastIndexOf('.') + 1),
                            IsSaved = false
                        };

                        Request.Files[0].SaveAs(photoModel.TempFullFileName);

                        checkResult.PhotoList.Add(photoModel);

                        Session["FormModel"] = model;

                        result.Success();
                    }
                    else if ((Define.EnumFormAction)Session["FormAction"] == Define.EnumFormAction.Edit)
                    {
                        var model = Session["FormModel"] as EditFormModel;

                        var checkItem = new CheckItemModel();

                        foreach (var checkType in model.CheckTypeList)
                        {
                            checkItem = checkType.CheckItemList.FirstOrDefault(x => x.UniqueID == CheckItemUniqueID);

                            if (checkItem != null)
                            {
                                break;
                            }
                        }

                        var checkResult = checkItem.CheckResultList.First(x => x.Seq == CheckItemSeq);

                        if (checkResult == null)
                        {
                            checkResult = new CheckResultModel()
                            {
                                Seq = CheckItemSeq
                            };

                            checkItem.CheckResultList.Add(checkResult);
                        }

                        int seq = 1;

                        if (checkResult.PhotoList.Count > 0)
                        {
                            seq = model.PhotoList.Max(x => x.Seq) + 1;
                        }

                        var photoName = Request.Files[0].FileName.Substring(Request.Files[0].FileName.LastIndexOf('\\') + 1);

                        var photoModel = new PhotoModel()
                        {
                            TempUniqueID = Guid.NewGuid().ToString(),
                            Seq = seq,
                            FormUniqueID = model.UniqueID,
                            CheckItemUniqueID = CheckItemUniqueID,
                            CheckItemSeq = CheckItemSeq,
                            Extension = photoName.Substring(photoName.LastIndexOf('.') + 1),
                            IsSaved = false
                        };

                        Request.Files[0].SaveAs(photoModel.TempFullFileName);

                        checkResult.PhotoList.Add(photoModel);

                        Session["FormModel"] = model;

                        result.Success();
                    }
                    else
                    {
                        result.ReturnFailedMessage(Resources.Resource.UnKnownOperation);
                    }
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

        public ActionResult Report(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                result = CheckListDataAccessor.Report(selectedList);

                if (result.IsSuccess)
                {
                    Session["Report"] = result.Data;
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

        public ActionResult MonthlyReport(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                result = CheckListDataAccessor.MonthlyReport(selectedList);

                if (result.IsSuccess)
                {
                    Session["MonthlyReport"] = result.Data;
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

        public ActionResult MonthlyReportDownload()
        {
            var excel = Session["MonthlyReport"] as ExcelExportModel;

            Session.Remove("MonthlyReport");

            return File(excel.FullFileName, excel.ContentType, excel.FileName);
        }

        public ActionResult ReportDownload()
        {
            var excel = Session["Report"] as ExcelExportModel;

            Session.Remove("Report");

            return File(excel.FullFileName, excel.ContentType, excel.FileName);
        }
    }
}
#endif