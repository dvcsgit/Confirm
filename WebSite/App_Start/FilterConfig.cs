using System.Web;
using System.Web.Mvc;
using WebSite.Filters;

namespace WebSite
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AjaxAuthorizeAttribute());
            filters.Add(new AuthorizeAttribute());
            filters.Add(new PermissionAttribute());
            filters.Add(new CultureActionFilter());
            filters.Add(new BreadCrumbAttribute());
            filters.Add(new PageHeaderAttribute());
        }
    }
}