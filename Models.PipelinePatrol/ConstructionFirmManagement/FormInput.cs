using System.ComponentModel.DataAnnotations;

namespace Models.PipelinePatrol.ConstructionFirmManagement
{
    public class FormInput
    {
        [Display(Name = "ConstructionFirmID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "ConstructionFirmIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "ConstructionFirmName", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "ConstructionFirmNameRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }
    }
}
