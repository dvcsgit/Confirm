#if PipelinePatrol
using DataAccess.PipelinePatrol;
using Models.Authenticated;
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

namespace WebSite.Areas.PipelinePatrol.Controllers
{
    public class DashboardController : Controller
    {
        // GET: PipelinePatrol/Dashboard
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Query(string Zoom, string MapCenterLAT, string MapCenterLNG, string Temp)
        {
            try
            {
                List<string> selectedList = JsonConvert.DeserializeObject<List<string>>(Temp);

                RequestResult result = DashboardDataAccessor.Query(Zoom, MapCenterLAT, MapCenterLNG, selectedList, Session["Account"] as Account);

                if (result.IsSuccess)
                {
                    return PartialView("_Dashboard", result.Data);
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

        public ActionResult InitTree()
        {
            try
            {
                RequestResult result = DashboardDataAccessor.GetTreeItem(Define.EnumTreeNodeType.Organization, "*", "", Session["Account"] as Account);

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

        public ActionResult GetTreeItem(Define.EnumTreeNodeType NodeType, string OrganizationUniqueID, string PipePointType)
        {
            string jsonTree = string.Empty;

            try
            {
                RequestResult result = DashboardDataAccessor.GetTreeItem(NodeType, OrganizationUniqueID, PipePointType, Session["Account"] as Account);

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