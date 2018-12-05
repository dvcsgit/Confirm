using System.ComponentModel.DataAnnotations;

namespace Models.OrganizationManagement
{
    public class FormInput
    {
        [Display(Name = "OrganizationID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "OrganizationIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "OrganizationDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "OrganizationDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [Display(Name = "Manager", ResourceType = typeof(Resources.Resource))]
        public string Managers { get; set; }
    }
}
