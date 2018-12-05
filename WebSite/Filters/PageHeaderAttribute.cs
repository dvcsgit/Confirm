using System;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Reflection;
using Utility;
using Models.Authenticated;

namespace WebSite.Filters
{
    public class PageHeaderAttribute : ActionFilterAttribute, IActionFilter
    {
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            var account = HttpContext.Current.Session["Account"] as Account;

            try
            {
                string area = string.Empty;

                if (filterContext.RouteData.DataTokens["area"] != null)
                {
                    area = filterContext.RouteData.DataTokens["area"].ToString();
                }

                string controller = filterContext.RouteData.Values["controller"].ToString();

                if (controller != "Home")
                {
                    if (account != null)
                    {
                        var parentPage = account.MenuItemList.FirstOrDefault(a => a.SubItemList.Any(x =>x.Area==area&& x.Controller == controller));

                        if (parentPage != null)
                        {
                            var page = parentPage.SubItemList.FirstOrDefault(x => x.Area == area && x.Controller == controller);

                            if (page != null)
                            {
                                //filterContext.Controller.ViewBag.Controller = page.ID;
                                filterContext.Controller.ViewBag.FunctionName = page.Description[filterContext.Controller.ViewBag.Lang];
                            }
                            else
                            {
                                //filterContext.Controller.ViewBag.Controller = null;
                                filterContext.Controller.ViewBag.FunctionName = null;
                            }
                        }
                        else
                        {
                            //filterContext.Controller.ViewBag.Controller = null;
                            filterContext.Controller.ViewBag.FunctionName = null;
                        }
                    }
                    else
                    {
                        //filterContext.Controller.ViewBag.Controller = null;
                        filterContext.Controller.ViewBag.FunctionName = null;
                    }
                }
                else
                {
                    //filterContext.Controller.ViewBag.Controller = null;
                    filterContext.Controller.ViewBag.FunctionName = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(MethodBase.GetCurrentMethod(), ex);

                //filterContext.Controller.ViewBag.Controller = null;
                filterContext.Controller.ViewBag.FunctionName = null;
            }
        }

        void IActionFilter.OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}