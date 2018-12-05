using System.Web;
using System.Web.Mvc;

namespace WebApi.PipelinePatrol
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}