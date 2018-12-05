using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.EquipmentCheckItemManagement
{
    public class FormInput
    {
        [Display(Name = "EquipmentID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "EquipmentIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "EquipmentName", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "EquipmentNameRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = "IsFeelItemDefaultNormal", ResourceType = typeof(Resources.Resource))]
        public bool IsFeelItemDefaultNormal { get; set; }
    }
}
