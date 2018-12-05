using System.Web.Mvc;

namespace WebSite.Areas.Customized_ASE
{
    public class Customized_ASEAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Customized_ASE";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Customized_ASE_default",
                "{lang}/Customized_ASE/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}