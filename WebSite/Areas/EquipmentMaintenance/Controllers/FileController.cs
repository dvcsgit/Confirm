using System;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utility;
using Utility.Models;
using Models.Shared;
using Models.Authenticated;
using Models.EquipmentMaintenance.FileManagement;
using System.IO;

#if ASE
using DataAccess.ASE;
#else
using DataAccess.EquipmentMaintenance;
using DataAccess;
#endif

using System.Web;

namespace WebSite.Areas.EquipmentMaintenance.Controllers
{
    public class FileController : Controller
    {
        public ActionResult Index()
        {
            return View(new QueryFormModel());
        }

        public ActionResult Query(QueryParameters Parameters)
        {
            RequestResult result = FileDataAccessor.Query(Parameters, Session["Account"] as Account);

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
            RequestResult result = FileDataAccessor.GetDetailViewModel(UniqueID);

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
        public ActionResult Create(string OrganizationUniqueID, string EquipmentUniqueID, string PartUniqueID, string MaterialUniqueID, string FolderUniqueID)
        {
            return PartialView("_Create", new CreateFormModel()
            {
                OrganizationUniqueID = OrganizationUniqueID,
                EquipmentUniqueID = EquipmentUniqueID,
                PartUniqueID = PartUniqueID,
                MaterialUniqueID=MaterialUniqueID,
                FolderUniqueID = FolderUniqueID
            });
        }

        [HttpPost]
        public ActionResult Create(CreateFormModel Model)
        {
            RequestResult result = new RequestResult();

            try
            {
                if (Model.FormInput.File != null && Model.FormInput.File.ContentLength > 0)
                {
                    string extension = Model.FormInput.File.FileName.Substring(Model.FormInput.File.FileName.LastIndexOf('.') + 1);

                    var guid = Guid.NewGuid().ToString();

                    if (string.IsNullOrEmpty(Model.FormInput.FileName))
                    {
                        Model.FormInput.FileName = Model.FormInput.File.FileName.Substring(0, Model.FormInput.File.FileName.LastIndexOf('.'));
                    }

                    Model.FormInput.File.SaveAs(Path.Combine(Config.EquipmentMaintenanceFileFolderPath, guid + "." + extension));

                    result = FileDataAccessor.Create(Model, guid, extension, Model.FormInput.File.ContentLength, Session["Account"] as Account);

                    if (!result.IsSuccess)
                    {
                        System.IO.File.Delete(Path.Combine(Config.EquipmentMaintenanceFileFolderPath, guid + "." + extension));
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

            ViewBag.RequestResult = result;

            return View("Index", new QueryFormModel()
            {
                Parameters = new QueryParameters()
                {
                    OrganizationUniqueID = Model.OrganizationUniqueID,
                    EquipmentUniqueID = Model.EquipmentUniqueID,
                    PartUniqueID = Model.PartUniqueID,
                    MaterialUniqueID=Model.MaterialUniqueID,
                    FolderUniqueID = Model.FolderUniqueID
                }
            });
        }

        [HttpGet]
        public ActionResult Edit(string UniqueID)
        {
            RequestResult result = FileDataAccessor.GetEditFormModel(UniqueID);

            if (result.IsSuccess)
            {
                return PartialView("_Edit", result.Data);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        [HttpPost]
        public ActionResult Edit(EditFormModel Model)
        {
            return Content(JsonConvert.SerializeObject(FileDataAccessor.Edit(Model, Session["Account"] as Account)));
        }

        public ActionResult Delete(string Selecteds)
        {
            RequestResult result = new RequestResult();

            try
            {
                var selectedList = JsonConvert.DeserializeObject<List<string>>(Selecteds);

                result = FileDataAccessor.Delete(selectedList);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                result.ReturnError(err);
            }

            return Content(JsonConvert.SerializeObject(result));
        }

        public ActionResult Download(string UniqueID)
        {
            var file = FileDataAccessor.Get(UniqueID);

            return File(file.FullFileName, file.ContentType, file.FileName);
        }

        public ActionResult InitTree(string RefOrganizationUniqueID)
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                var account = Session["Account"] as Account;

                RequestResult result = new RequestResult();

                if (account.UserRootOrganizationUniqueID == "*")
                {
                    result = FileDataAccessor.GetTreeItem(organizationList, account.UserRootOrganizationUniqueID, "", "", "", "", "", Session["Account"] as Account);
                }
                else
                {
                    result = FileDataAccessor.GetRootTreeItem(organizationList, account.UserRootOrganizationUniqueID, Session["Account"] as Account);
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

        public ActionResult GetTreeItem(string OrganizationUniqueID, string EquipmentUniqueID, string PartUniqueID, string MaterialType, string MaterialUniqueID, string FolderUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = FileDataAccessor.GetTreeItem(organizationList, OrganizationUniqueID, EquipmentUniqueID, PartUniqueID, MaterialType, MaterialUniqueID, FolderUniqueID, Session["Account"] as Account);

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