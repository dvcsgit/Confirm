using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.FolderManagement
{
    public class FormInput
    {
        [Display(Name = "FolderDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "FolderDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
    }
}
