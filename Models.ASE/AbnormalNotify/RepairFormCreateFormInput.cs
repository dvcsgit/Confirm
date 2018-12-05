using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.AbnormalNotify
{
    public class RepairFormCreateFormInput
    {
        [Display(Name = "MaintenanceOrganization", ResourceType = typeof(Resources.Resource))]
        public string MaintenanceOrganizationUniqueID { get; set; }

        [Display(Name = "Equipment", ResourceType = typeof(Resources.Resource))]
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
