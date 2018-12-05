using System.Web.Mvc;

namespace WebSite.Areas.AbnormalNotify
{
    public class AbnormalNotifyAreaRegistration : AreaRegistration 
    {
        public override string AreaName
        {
            get
            {
                return "AbnormalNotify";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "AbnormalNotify_default",
                "{lang}/AbnormalNotify/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}