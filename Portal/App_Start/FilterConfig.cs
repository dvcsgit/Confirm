using System.Web;
using System.Web.Mvc;
using Portal.Filters;

namespace Portal
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new CultureActionFilter());
        }
    }
}