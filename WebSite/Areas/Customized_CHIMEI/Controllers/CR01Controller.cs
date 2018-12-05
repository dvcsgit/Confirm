#if CHIMEI
using Customized.CHIMEI.DataAccess;
using Customized.CHIMEI.Models.AIMSJobQuery;
using DataAccess;
using Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Utility;
using Utility.Models;

namespace WebSite.Areas.Customized_CHIMEI.Controllers
{
    [AllowAnonymous]
    public class CR01Controller : Controller
    {
        public ActionResult Index(string VHNO)
        {
            return View(new QueryFormModel()
            {
                Parameters = new QueryParameters()
                {
                    VHNO = VHNO
                }
            });
        }

        public ActionResult Query(QueryFormModel Model)
        {
            RequestResult result = AIMSJobQueryHelper.Query(Model.Parameters);

            if (result.IsSuccess)
            {
                Session["QueryResults"] = result.Data;

                return PartialView("_JobList", Session["QueryResults"]);
            }
            else
            {
                return PartialView("_Error", result.Error);
            }
        }

        public ActionResult Export(string VHNO)
        {
            var model = AIMSJobQueryHelper.Export(Session["QueryResults"] as List<AIMSJobModel>, VHNO).Data as ExcelExportModel;

            return File(model.Data, model.ContentType, model.FileName);
        }

        public ActionResult Back2JobList()
        {
            return PartialView("_JobList", Session["QueryResults"]);
        }

        public ActionResult GetEquipmentList(string VHNO)
        {
            try
            {
                var aimsJobList = Session["QueryResults"] as List<AIMSJobModel>;

                var aimsJob = aimsJobList.First(x => x.VHNO == VHNO);

                Session["AIMSJOB"] = aimsJob;

                return PartialView("_EquipmentList", Session["AIMSJOB"]);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult Back2EquipmentList()
        {
            return PartialView("_EquipmentList", Session["AIMSJOB"]);
        }

        public ActionResult GetCheckItemList(string EquipmentUniqueID)
        {
            try
            {
                var aimsJob = Session["AIMSJOB"] as AIMSJobModel;

                var equipment = aimsJob.EquipmentList.First(x => x.UniqueID == EquipmentUniqueID);

                Session["AIMSEquipment"] = equipment;

                return PartialView("_CheckItemList", Session["AIMSEquipment"]);
            }
            catch (Exception ex)
            {
                var err = new Error(MethodBase.GetCurrentMethod(), ex);

                Logger.Log(err);

                return PartialView("_Error", err);
            }
        }

        public ActionResult InitTree()
        {
            try
            {
                var organizationList = HttpRuntime.Cache.GetOrInsert<List<Models.Shared.Organization>>("Organizations", () => OrganizationDataAccessor.GetAllOrganization());

                RequestResult result = new RequestResult();

                result = AIMSJobQueryHelper.GetTreeItem(organizationList, "*");

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

                RequestResult result = AIMSJobQueryHelper.GetTreeItem(organizationList, OrganizationUniqueID);

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

        public ActionResult EditAbnormalReason(string UniqueID)
        {
            return PartialView("_EditAbnormalReason", UniqueID);
        }

        public ActionResult SaveAbnormalReason(string UniqueID, string AbnormalReason, string HandlingMethod)
        {
            RequestResult result = AIMSJobQueryHelper.SaveAbnormalReason(UniqueID, AbnormalReason, HandlingMethod);

            if (result.IsSuccess)
            {
                var equipment = Session["AIMSEquipment"] as EquipmentModel;

                result.Data = equipment.UniqueID;

                foreach (var checkItem in equipment.CheckItemList)
                {
                    var checkResult = checkItem.CheckResultList.FirstOrDefault(x => x.UniqueID == UniqueID);

                    if (checkResult != null)
                    {
                        checkResult.AbnormalReasonList = new List<AbnormalReasonModel>() { 
                        new AbnormalReasonModel()
                        {
                            Description = Resources.Resource.Other,
                            Remark = AbnormalReason,
                            HandlingMethodList = new List<HandlingMethodModel>() { 
                                 new HandlingMethodModel(){
                                  Description=Resources.Resource.Other,
                                   Remark=HandlingMethod
                                 }
                            }
                        }
                        };

                        break;
                    }
                }
            }

            return Content(JsonConvert.SerializeObject(result));
        }
    }
}
#endif