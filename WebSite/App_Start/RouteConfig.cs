using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebSite
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

#if PFG_CN
            routes.MapRoute(
               name: "Default",
               url: "{lang}/{controller}/{action}/{id}",
                defaults: new { lang = "zh-cn", controller = "Home", action = "Login", id = UrlParameter.Optional }
           );
#else
             routes.MapRoute(
               name: "Default",
               url: "{lang}/{controller}/{action}/{id}",
                defaults: new { lang = "zh-tw", controller = "Home", action = "Login", id = UrlParameter.Optional }
           );
#endif
        }
    }
}