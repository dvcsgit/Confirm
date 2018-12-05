using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.EquipmentCheckItemManagement
{
    public class PartFormInput
    {
        [Display(Name = "PartDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "PartDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
    }
}
