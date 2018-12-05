using System.Web.Mvc;

namespace WebSite.Areas.Customized_FPTC
{
    public class Customized_FPTCAreaRegistration : AreaRegistration 
    {
        public override string AreaName
        {
            get
            {
                return "Customized_FPTC";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Customized_FPTC_default",
                "{lang}/Customized_FPTC/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}