using System;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Reflection;
using System.Net.Mime;
using Utility;
using Utility.Models;
using Models.Authenticated;

namespace WebSite.Filters
{
    public class PermissionAttribute : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var account = HttpContext.Current.Session["Account"] as Account;

                string area = string.Empty;

                if (filterContext.RouteData.DataTokens["area"] != null)
                {
                    area = filterContext.RouteData.DataTokens["area"].ToString();
                }

                string controller = filterContext.RouteData.Values["controller"].ToString();

                if (account != null)
                {
                    if (area != "Customized_CHIMEI" && controller != "Home")
                    {
                        if (!string.IsNullOrEmpty(area))
                        {
                            filterContext.Controller.ViewBag.CanCreate = account.WebPermissionFunctionList.Any(x => x.Area == area && x.Controller == controller && x.WebFunctionID == "Create");
                            filterContext.Controller.ViewBag.CanDelete = account.WebPermissionFunctionList.Any(x => x.Area == area && x.Controller == controller && x.WebFunctionID == "Delete");
                            filterContext.Controller.ViewBag.CanEdit = account.WebPermissionFunctionList.Any(x => x.Area == area && x.Controller == controller && x.WebFunctionID == "Edit");
                            filterContext.Controller.ViewBag.CanQuery = account.WebPermissionFunctionList.Any(x => x.Area == area && x.Controller == controller && x.WebFunctionID == "Query");
                            filterContext.Controller.ViewBag.CanClosed = account.WebPermissionFunctionList.Any(x => x.Area == area && x.Controller == controller && x.WebFunctionID == "Closed");
                            filterContext.Controller.ViewBag.CanDownload = account.WebPermissionFunctionList.Any(x => x.Area == area && x.Controller == controller && x.WebFunctionID == "Download");
                            filterContext.Controller.ViewBag.CanUpload = account.WebPermissionFunctionList.Any(x => x.Area == area && x.Controller == controller && x.WebFunctionID == "Upload");
                            filterContext.Controller.ViewBag.CanVerify = account.WebPermissionFunctionList.Any(x => x.Area == area && x.Controller == controller && x.WebFunctionID == "Verify");
                            filterContext.Controller.ViewBag.CanTakeJob = account.WebPermissionFunctionList.Any(x => x.Area == area && x.Controller == controller && x.WebFunctionID == "TakeJob");
                        }
                        else
                        {
                            filterContext.Controller.ViewBag.CanCreate = account.WebPermissionFunctionList.Any(x => x.Controller == controller && x.WebFunctionID == "Create");
                            filterContext.Controller.ViewBag.CanDelete = account.WebPermissionFunctionList.Any(x => x.Controller == controller && x.WebFunctionID == "Delete");
                            filterContext.Controller.ViewBag.CanEdit = account.WebPermissionFunctionList.Any(x => x.Controller == controller && x.WebFunctionID == "Edit");
                            filterContext.Controller.ViewBag.CanQuery = account.WebPermissionFunctionList.Any(x => x.Controller == controller && x.WebFunctionID == "Query");
                            filterContext.Controller.ViewBag.CanClosed = account.WebPermissionFunctionList.Any(x => x.Controller == controller && x.WebFunctionID == "Closed");
                            filterContext.Controller.ViewBag.CanDownload = account.WebPermissionFunctionList.Any(x => x.Controller == controller && x.WebFunctionID == "Download");
                            filterContext.Controller.ViewBag.CanUpload = account.WebPermissionFunctionList.Any(x => x.Controller == controller && x.WebFunctionID == "Upload");
                            filterContext.Controller.ViewBag.CanVerify = account.WebPermissionFunctionList.Any(x => x.Controller == controller && x.WebFunctionID == "Verify");
                            filterContext.Controller.ViewBag.CanTakeJob = account.WebPermissionFunctionList.Any(x => x.Controller == controller && x.WebFunctionID == "TakeJob");
                        }
                    }
                }
                else
                {
                    filterContext.Controller.ViewBag.CanCreate = false;
                    filterContext.Controller.ViewBag.CanDelete = false;
                    filterContext.Controller.ViewBag.CanEdit = false;
                    filterContext.Controller.ViewBag.CanQuery = false;
                    filterContext.Controller.ViewBag.CanClosed = false;
                    filterContext.Controller.ViewBag.CanDownload = false;
                    filterContext.Controller.ViewBag.CanUpload = false;
                    filterContext.Controller.ViewBag.CanVerify = false;
                    filterContext.Controller.ViewBag.CanTakeJob = false;
                }
            }
            catch (Exception ex)
            {
                filterContext.Controller.ViewBag.CanCreate = false;
                filterContext.Controller.ViewBag.CanDelete = false;
                filterContext.Controller.ViewBag.CanEdit = false;
                filterContext.Controller.ViewBag.CanQuery = false;
                filterContext.Controller.ViewBag.CanClosed = false;
                filterContext.Controller.ViewBag.CanDownload = false;
                filterContext.Controller.ViewBag.CanUpload = false;
                filterContext.Controller.ViewBag.CanVerify = false;
                filterContext.Controller.ViewBag.CanTakeJob = false;

                Logger.Log(MethodBase.GetCurrentMethod(), ex);
            }
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}