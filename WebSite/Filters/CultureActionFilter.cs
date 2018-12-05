using System.Web.Mvc;
using System.Threading;
using System.Globalization;

namespace WebSite.Filters
{
    public class CultureActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            string lang;

            if (filterContext.RouteData.Values.ContainsKey("Lang"))
            {
                lang = filterContext.RouteData.Values["Lang"].ToString().ToLower();
            }
            else
            {
#if PFG_CN
                lang = "zh-cn";
#else
                lang = "zh-tw";
#endif
            }

            filterContext.Controller.ViewBag.Lang = lang;

            Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
        }
    }
}