using System.Web.Mvc;

namespace WebSite.Areas.Customized_LCY
{
    public class Customized_LCYAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Customized_LCY";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Customized_LCY_default",
                "{lang}/Customized_LCY/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}