using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.AbnormalHandlingManagement
{
    public class QueryParameters
    {
        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string EquipmentUniqueID { get; set; }

        public string PartUniqueID { get; set; }

        public string BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(BeginDateString);
            }
        }

        [Display(Name = "BeginDate", ResourceType = typeof(Resources.Resource))]
        public string BeginDateString { get; set; }


        public string EndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(EndDateString);
            }
        }

        [Display(Name = "EndDate", ResourceType = typeof(Resources.Resource))]
        public string EndDateString { get; set; }

        public string Status { get; set; }

        public string ClosedStatus { get; set; }

        public string AbnormalType { get; set; }

        public string Type { get; set; }
    }
}
