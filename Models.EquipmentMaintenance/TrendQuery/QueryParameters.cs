using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.TrendQuery
{
    public class QueryParameters
    {
        public string ControlPointUniqueID { get; set; }

        public string EquipmentUniqueID { get; set; }

        public string PartUniqueID { get; set; }

        public string CheckItemUniqueID { get; set; }

        [Display(Name = "BeginDate", ResourceType = typeof(Resources.Resource))]
        public string BeginDateString { get; set; }

        public string BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(BeginDateString);
            }
        }

        [Display(Name = "EndDate", ResourceType = typeof(Resources.Resource))]
        public string EndDateString { get; set; }

        public string EndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(EndDateString);
            }
        }
    }
}
