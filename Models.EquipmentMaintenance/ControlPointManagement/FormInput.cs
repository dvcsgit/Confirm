using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.ControlPointManagement
{
    public class FormInput
    {
        [Display(Name = "ControlPointID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "ControlPointIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "ControlPointDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "ControlPointDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [Display(Name = "TagID", ResourceType = typeof(Resources.Resource))]
        public string TagID { get; set; }

        [Display(Name = "IsFeelItemDefaultNormal", ResourceType = typeof(Resources.Resource))]
        public bool IsFeelItemDefaultNormal { get; set; }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }
    }
}
