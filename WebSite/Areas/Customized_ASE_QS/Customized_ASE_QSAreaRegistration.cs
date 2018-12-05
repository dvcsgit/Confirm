using System.Web.Mvc;

namespace WebSite.Areas.Customized_ASE_QS
{
    public class Customized_ASE_QSAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Customized_ASE_QS";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Customized_ASE_QS_default",
                "{lang}/Customized_ASE_QS/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}