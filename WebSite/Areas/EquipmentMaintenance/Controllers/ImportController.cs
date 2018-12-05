using System;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.EquipmentMaintenance.Import;

#if ASE
using DataAccess.ASE;
#else
using DataAccess.EquipmentMaintenance;
#endif

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class ImportController : Controller
    {
        public ActionResult Download()
        {
            return View();
        }

        public ActionResult FileDownload(Define.EnumExcelVersion ExcelVersion)
        {
            var file = ImportHelper.GetFile(ExcelVersion);

            return File(file.Data, file.ContentType, file.FileName);
        }

        [HttpGet]
        public ActionResult Upload()
        {
            return View(new UploadFormModel());
        }

        [HttpPost]
        public ActionResult Upload(UploadFormModel Model)
        {
            RequestResult result = ImportHelper.GetProgressFormModel(Model);

            if (result.IsSuccess)
            {
                Session["ProgressFormModel"] = result.Data;

                return View("Progress", Session["ProgressFormModel"]);
            }
            else
            {
                return View("Upload", new UploadFormModel()
                {
                    InitialMessage = result.Message
                });
            }
        }

        public ActionResult Import()
        {
            return Content(JsonConvert.SerializeObject(ImportHelper.Import(Session["ImportModel"] as ImportModel)));
        }

        public ActionResult GetRowData(int SheetIndex, int RowIndex)
        {
            RequestResult result = new RequestResult();

            try
            {
                var model = Session["ProgressFormModel"] as ProgressFormModel;

                result = ImportHelper.GetRowData(model, SheetIndex, RowIndex);

                if (result.IsSuccess)
                {
                    Session["ProgressFormModel"] = result.Data;
                }
                else
                {
                    Session["ProgressFormModel"] = model;
                }

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

        public ActionResult Validate()
        {
            try
            {
                RequestResult result = ImportHelper.Validate(Session["ProgressFormModel"] as ProgressFormModel);

                if (result.IsSuccess)
                {
                    Session.Remove("ProgressFormModel");

                    Session["ImportModel"] = result.Data;

                    return PartialView("_Import", Session["ImportModel"]);
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

        public ActionResult InitOrganizationTree()
        {
            try
            {
                RequestResult result = ImportHelper.GetOrganizationTreeItem(Session["ImportModel"] as ImportModel, "*");

                if (result.IsSuccess)
                {
                    return PartialView("_OrganizationTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetOrganizationTreeItem(string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = ImportHelper.GetOrganizationTreeItem(Session["ImportModel"] as ImportModel, OrganizationUniqueID);

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

        public ActionResult InitUserTree()
        {
            try
            {
                RequestResult result = ImportHelper.GetUserTreeItem(Session["ImportModel"] as ImportModel, "*");

                if (result.IsSuccess)
                {
                    return PartialView("_UserTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetUserTreeItem(string OrganizationUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = ImportHelper.GetUserTreeItem(Session["ImportModel"] as ImportModel, OrganizationUniqueID);

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

        public ActionResult InitRouteTree()
        {
            try
            {
                RequestResult result = ImportHelper.GetRouteTreeItem(Session["ImportModel"] as ImportModel, "*", "", "", "", "");

                if (result.IsSuccess)
                {
                    return PartialView("_RouteTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetRouteTreeItem(string OrganizationUniqueID, string RouteUniqueID, string ControlPointUniqueID, string EquipmentUniqueID, string PartUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = ImportHelper.GetRouteTreeItem(Session["ImportModel"] as ImportModel, OrganizationUniqueID, RouteUniqueID, ControlPointUniqueID, EquipmentUniqueID, PartUniqueID);

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

        public ActionResult InitControlPointTree()
        {
            try
            {
                RequestResult result = ImportHelper.GetControlPointTreeItem(Session["ImportModel"] as ImportModel, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_ControlPointTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetControlPointTreeItem(string OrganizationUniqueID, string ControlPointUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = ImportHelper.GetControlPointTreeItem(Session["ImportModel"] as ImportModel, OrganizationUniqueID, ControlPointUniqueID);

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

        public ActionResult InitEquipmentTree()
        {
            try
            {
                RequestResult result = ImportHelper.GetEquipmentTreeItem(Session["ImportModel"] as ImportModel, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_EquipmentTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetEquipmentTreeItem(string OrganizationUniqueID, string EquipmentUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = ImportHelper.GetEquipmentTreeItem(Session["ImportModel"] as ImportModel, OrganizationUniqueID, EquipmentUniqueID);

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

        public ActionResult InitCheckItemTree()
        {
            try
            {
                RequestResult result = ImportHelper.GetCheckItemTreeItem(Session["ImportModel"] as ImportModel, "*", "", "");

                if (result.IsSuccess)
                {
                    return PartialView("_CheckItemTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetCheckItemTreeItem(string OrganizationUniqueID, string CheckType, string CheckItemUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = ImportHelper.GetCheckItemTreeItem(Session["ImportModel"] as ImportModel, OrganizationUniqueID, CheckType, CheckItemUniqueID);

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

        public ActionResult InitAbnormalReasonTree()
        {
            try
            {
                RequestResult result = ImportHelper.GetAbnormalReasonTreeItem(Session["ImportModel"] as ImportModel, "*", "", "");

                if (result.IsSuccess)
                {
                    return PartialView("_AbnormalReasonTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetAbnormalReasonTreeItem(string OrganizationUniqueID, string AbnormalType, string AbnormalReasonUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = ImportHelper.GetAbnormalReasonTreeItem(Session["ImportModel"] as ImportModel, OrganizationUniqueID, AbnormalType, AbnormalReasonUniqueID);

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

        public ActionResult InitHandlingMethodTree()
        {
            try
            {
                RequestResult result = ImportHelper.GetHandlingMethodTreeItem(Session["ImportModel"] as ImportModel, "*", "");

                if (result.IsSuccess)
                {
                    return PartialView("_HandlingMethodTree", JsonConvert.SerializeObject((List<TreeItem>)result.Data));
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

        public ActionResult GetHandlingMethodTreeItem(string OrganizationUniqueID, string HandlingMethodType)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = ImportHelper.GetHandlingMethodTreeItem(Session["ImportModel"] as ImportModel, OrganizationUniqueID, HandlingMethodType);

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
    }
}