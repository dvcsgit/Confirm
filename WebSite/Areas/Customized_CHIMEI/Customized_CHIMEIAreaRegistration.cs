using System.Web.Mvc;

namespace WebSite.Areas.Customized_CHIMEI
{
    public class Customized_CHIMEIAreaRegistration : AreaRegistration 
    {
        public override string AreaName
        {
            get
            {
                return "Customized_CHIMEI";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Customized_CHIMEI_default",
                "{lang}/Customized_CHIMEI/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}