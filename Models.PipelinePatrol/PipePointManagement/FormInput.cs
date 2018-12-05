using System.ComponentModel.DataAnnotations;

namespace Models.PipelinePatrol.PipePointManagement
{
    public class FormInput
    {
        [Display(Name = "PipePointType", ResourceType = typeof(Resources.Resource))]
        public string PipePointType { get; set; }

        [Display(Name = "PipePointID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "PipePointIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "PipePointName", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "PipePointNameRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = "LAT", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "PipePointLocationRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public double? LAT { get; set; }

        [Display(Name = "LNG", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "PipePointLocationRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public double? LNG { get; set; }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }
    }
}
