using System.Web.Mvc;

namespace WebSite.Areas.Customized_PFG
{
    public class Customized_PFGAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Customized_PFG";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Customized_PFG_default",
                "{lang}/Customized_PFG/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}