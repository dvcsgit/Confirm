using System.Web.Mvc;

namespace WebSite.Areas.EquipmentMaintenance
{
    public class EquipmentMaintenanceAreaRegistration : AreaRegistration 
    {
        public override string AreaName
        {
            get
            {
                return "EquipmentMaintenance";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "EquipmentMaintenance_default",
                "{lang}/EquipmentMaintenance/{controller}/{action}/{id}",
                new { lang = "zh-tw", id = UrlParameter.Optional }
            );
        }
    }
}