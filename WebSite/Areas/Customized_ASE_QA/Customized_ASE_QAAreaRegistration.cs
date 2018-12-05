using System.Web.Mvc;

namespace WebSite.Areas.Customized_ASE_QA
{
    public class Customized_ASE_QAAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Customized_ASE_QA";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Customized_ASE_QA_default",
                "{lang}/Customized_ASE_QA/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}