using System.ComponentModel.DataAnnotations;

namespace Models.GuardPatrol.HandlingMethodManagement
{
    public class FormInput
    {
        [Display(Name = "HandlingMethodType", ResourceType = typeof(Resources.Resource))]
        public string HandlingMethodType { get; set; }

        [Display(Name = "HandlingMethodID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "HandlingMethodIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "HandlingMethodDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "HandlingMethodDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
    }
}
