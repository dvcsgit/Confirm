using System.Web.Mvc;

namespace WebSite.Areas.Customized_PFG_CN
{
    public class Customized_PFG_CNAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Customized_PFG_CN";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Customized_PFG_CN_default",
                "{lang}/Customized_PFG_CN/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}