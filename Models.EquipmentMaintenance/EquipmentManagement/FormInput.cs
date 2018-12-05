using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.EquipmentManagement
{
    public class FormInput
    {
        [Display(Name = "EquipmentID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "EquipmentIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "EquipmentName", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "EquipmentNameRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = "MaintenanceOrganization", ResourceType = typeof(Resources.Resource))]
        public string MaintenanceOrganizationUniqueID { get; set; }
    }
}
