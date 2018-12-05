using System.Web.Mvc;

namespace WebSite.Areas.TruckPatrol
{
    public class TruckPatrolAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "TruckPatrol";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "TruckPatrol_default",
                "{lang}/TruckPatrol/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}