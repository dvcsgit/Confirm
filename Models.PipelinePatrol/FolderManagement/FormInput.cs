using System.ComponentModel.DataAnnotations;

namespace Models.PipelinePatrol.FolderManagement
{
    public class FormInput
    {
        [Display(Name = "FolderDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "FolderDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
    }
}
