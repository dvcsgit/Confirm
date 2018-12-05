using System.Web.Mvc;

namespace WebSite.Areas.Customized_SESC
{
    public class Customized_SESCAreaRegistration : AreaRegistration 
    {
        public override string AreaName
        {
            get
            {
                return "Customized_SESC";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Customized_SESC_default",
                "{lang}/Customized_SESC/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}