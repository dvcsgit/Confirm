using System.Web.Mvc;

namespace WebSite.Areas.TankPatrol
{
    public class TankPatrolAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "TankPatrol";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "TankPatrol_default",
                "{lang}/TankPatrol/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}