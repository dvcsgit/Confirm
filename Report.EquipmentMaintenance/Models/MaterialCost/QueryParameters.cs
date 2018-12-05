using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Report.EquipmentMaintenance.Models.MaterialCost
{
    public class QueryParameters
    {
        public string OrganizationUniqueID { get; set; }

        [Display(Name = "BeginDate", ResourceType = typeof(Resources.Resource))]
        public string BeginDateString { get; set; }

        public DateTime? BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(BeginDateString);
            }
        }

        [Display(Name = "EndDate", ResourceType = typeof(Resources.Resource))]
        public string EndDateString { get; set; }

        public DateTime? EndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString);
            }
        }
    }
}
