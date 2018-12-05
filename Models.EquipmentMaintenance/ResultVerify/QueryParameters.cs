using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.ResultVerify
{
    public class QueryParameters
    {
        public string OrganizationUniqueID { get; set; }

        public string RouteUniqueID { get; set; }

        public string JobUniqueID { get; set; }

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
