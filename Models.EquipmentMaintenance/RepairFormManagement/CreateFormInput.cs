using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.RepairFormManagement
{
    public class CreateFormInput
    {
        [Display(Name = "MaintenanceOrganization", ResourceType = typeof(Resources.Resource))]
        public string MaintenanceOrganizationUniqueID { get; set; }

        public string EquipmentUniqueID { get; set; }

        public string PartUniqueID { get; set; }

        [Display(Name = "RepairFormType", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "RepairFormTypeRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string RepairFormTypeUniqueID { get; set; }

        [Display(Name = "Subject", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "SubjectRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Subject { get; set; }

        public string SubjectUniqueID { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [Display(Name = "RepairBySelf", ResourceType = typeof(Resources.Resource))]
        public bool IsRepairBySelf { get; set; }

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
