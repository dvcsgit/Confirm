using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.MaterialManagement
{
    public class FormInput
    {
        [Display(Name = "MaterialType", ResourceType = typeof(Resources.Resource))]
        public string EquipmentType { get; set; }

        [Display(Name = "MaterialID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "MaterialIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "MaterialName", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "MaterialNameRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = "QTY", ResourceType = typeof(Resources.Resource))]
        public int? Quantity { get; set; }
    }
}
