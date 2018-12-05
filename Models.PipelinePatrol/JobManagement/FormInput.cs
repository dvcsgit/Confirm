using System.ComponentModel.DataAnnotations;

namespace Models.PipelinePatrol.JobManagement
{
    public class FormInput
    {
        [Display(Name = "JobID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "JobIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "JobDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "JobDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
    }
}
