using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.MonthlyReport
{
    public class QueryParameters
    {
        public string RouteUniqueID { get; set; }

        [Display(Name = "Year", ResourceType = typeof(Resources.Resource))]
        public string Year { get; set; }

        [Display(Name = "Month", ResourceType = typeof(Resources.Resource))]
        public string Month { get; set; }

        public string Ym
        {
            get
            {
                return string.Format("{0}{1}", Year, Month);
            }
        }
    }
}
