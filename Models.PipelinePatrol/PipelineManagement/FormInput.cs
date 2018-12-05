using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.PipelineManagement
{
    public class FormInput
    {
        [Display(Name = "PipelineID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "PipelineIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "PipelineColor", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "PipelineColorRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Color { get; set; }
    }
}
