using System.Web.Mvc;

namespace WebSite.Areas.GuardPatrol
{
    public class GuardPatrolAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "GuardPatrol";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "GuardPatrol_default",
                "{lang}/GuardPatrol/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}