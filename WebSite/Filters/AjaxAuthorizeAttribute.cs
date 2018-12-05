using System;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Reflection;
using Utility;
using Models.Authenticated;
using System.Web.Routing;

namespace WebSite.Filters
{
    public class AjaxAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext != null)
            {
                if (!httpContext.User.Identity.IsAuthenticated)
                    return false;
                else if (httpContext.Session["Account"] == null)
                    return false;
                else
                    return true;
            }
            else
            {
                return false;
            }
        }


        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (filterContext.Result is HttpUnauthorizedResult)
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    //var urlHelper = new UrlHelper(filterContext.RequestContext);

                    //filterContext.HttpContext.Response.StatusCode = 401;

                    //filterContext.Result = new JsonResult
                    //{
                    //    Data = new
                    //    {
                    //        Error = "NotAuthorized",
                    //        LogOnUrl = urlHelper.Action("Login", "Home")
                    //    },
                    //    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    //};

                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                            {
                                { "controller", "Home" },
                                { "action", "Login" }
                            });
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                            {
                                { "controller", "Home" },
                                { "action", "Login" }
                            });
                }
            }
        }
    }
}