using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Report.EquipmentMaintenance.Models.EquipmentRepairForm
{
    public class QueryParameters
    {
        [Display(Name = "Equipment", ResourceType = typeof(Resources.Resource))]
        public string Equipment { get; set; }

        [Display(Name = "RepairFormType", ResourceType = typeof(Resources.Resource))]
        public string RepairFormType { get; set; }

        [Display(Name = "Status", ResourceType = typeof(Resources.Resource))]
        public string Status { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "MaintenanceBeginDate", ResourceType = typeof(Resources.Resource))]
        public string EstBeginDateString { get; set; }

        public DateTime? EstBeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EstBeginDateString);
            }
        }

        [Display(Name = "MaintenanceEndDate", ResourceType = typeof(Resources.Resource))]
        public string EstEndDateString { get; set; }

        public DateTime? EstEndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EstEndDateString);
            }
        }
    }
}
