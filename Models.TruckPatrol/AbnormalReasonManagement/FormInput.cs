using System.ComponentModel.DataAnnotations;

namespace Models.TruckPatrol.AbnormalReasonManagement
{
    public class FormInput
    {
        [Display(Name = "AbnormalType", ResourceType = typeof(Resources.Resource))]
        public string AbnormalType { get; set; }

        [Display(Name = "AbnormalReasonID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "AbnormalReasonIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "AbnormalReasonDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "AbnormalReasonDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
    }
}
