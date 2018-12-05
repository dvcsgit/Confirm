using System.ComponentModel.DataAnnotations;

namespace Models.TruckPatrol.ResultQuery
{
    public class FormInput
    {
        [Display(Name = "AbnormalReasonID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "AbnormalReasonIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string UnPatrolReasonUniqueID { get; set; }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string UnPatrolReasonRemark { get; set; }
    }
}
