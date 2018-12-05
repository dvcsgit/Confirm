#if PipelinePatrol
using DataAccess.PipelinePatrol;
using Models.Authenticated;
using Models.PipelinePatrol.FileManagement;
using Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.PipelinePatrol.Controllers
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
        public ActionResult Create(string OrganizationUniqueID, string PipelineUniqueID, string PipePointUniqueID, string FolderUniqueID)
        {
            return PartialView("_Create", new CreateFormModel()
            {
                OrganizationUniqueID = OrganizationUniqueID,
                PipelineUniqueID = PipelineUniqueID,
                PipePointUniqueID = PipePointUniqueID,
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

                    Model.FormInput.File.SaveAs(Path.Combine(Config.PipelinePatrolFileFolderPath, guid + "." + extension));

                    result = FileDataAccessor.Create(Model, guid, extension, Model.FormInput.File.ContentLength, Session["Account"] as Account);

                    if (!result.IsSuccess)
                    {
                        System.IO.File.Delete(Path.Combine(Config.PipelinePatrolFileFolderPath, guid + "." + extension));
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
                    PipelineUniqueID = Model.PipelineUniqueID,
                    PipePointUniqueID = Model.PipePointUniqueID,
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

        public ActionResult InitTree()
        {
            try
            {
                RequestResult result = FileDataAccessor.GetTreeItem("*", "", "", "", "", Session["Account"] as Account);

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

        public ActionResult GetTreeItem(string OrganizationUniqueID, string PipelineUniqueID, string PipePointType, string PipePointUniqueID, string FolderUniqueID)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = FileDataAccessor.GetTreeItem(OrganizationUniqueID, PipelineUniqueID, PipePointType, PipePointUniqueID, FolderUniqueID, Session["Account"] as Account);

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
#endif