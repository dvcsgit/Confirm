using System.Web.Mvc;

namespace WebSite.Areas.PipelinePatrol
{
    public class PipelinePatrolAreaRegistration : AreaRegistration 
    {
        public override string AreaName
        {
            get
            {
                return "PipelinePatrol";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "PipelinePatrol_default",
                "{lang}/PipelinePatrol/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}