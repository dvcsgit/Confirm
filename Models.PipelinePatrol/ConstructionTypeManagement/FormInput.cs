using System.ComponentModel.DataAnnotations;

namespace Models.PipelinePatrol.ConstructionTypeManagement
{
    public class FormInput
    {
        [Display(Name = "ConstructionTypeID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "ConstructionTypeIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "ConstructionTypeDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "ConstructionTypeDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
    }
}
