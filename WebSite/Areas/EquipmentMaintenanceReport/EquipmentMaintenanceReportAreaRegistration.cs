using System.Web.Mvc;

namespace WebSite.Areas.EquipmentMaintenanceReport
{
    public class EquipmentMaintenanceReportAreaRegistration : AreaRegistration 
    {
        public override string AreaName
        {
            get
            {
                return "EquipmentMaintenanceReport";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "EquipmentMaintenanceReport_default",
                "{lang}/EquipmentMaintenanceReport/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}