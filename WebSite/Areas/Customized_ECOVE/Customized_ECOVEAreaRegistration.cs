using System.Web.Mvc;

namespace WebSite.Areas.Customized_ECOVE
{
    public class Customized_ECOVEAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Customized_ECOVE";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Customized_ECOVE_default",
                "Customized_ECOVE/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}