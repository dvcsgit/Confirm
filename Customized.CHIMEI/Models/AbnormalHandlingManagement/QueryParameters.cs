using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Customized.CHIMEI.Models.AbnormalHandlingManagement
{
    public class QueryParameters
    {
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
    }
}
