using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.QFormManagement
{
    public class CreateRepairFormInput
    {
        [Display(Name = "MaintenanceOrganization", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "MaintenanceOrganizationRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string MaintenanceOrganizationUniqueID { get; set; }

        public string EquipmentUniqueID { get; set; }

        public string PartUniqueID { get; set; }

        [Display(Name = "RepairFormType", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "RepairFormTypeRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string RepairFormTypeUniqueID { get; set; }

        [Display(Name = "Subject", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "SubjectRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Subject { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
    }
}
