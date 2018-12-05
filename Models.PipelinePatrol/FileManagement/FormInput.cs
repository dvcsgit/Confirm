using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Models.PipelinePatrol.FileManagement
{
    public class FormInput
    {
        [Display(Name = "FileName", ResourceType = typeof(Resources.Resource))]
        public string FileName { get; set; }

        [Display(Name = "IsDownload2Mobile", ResourceType = typeof(Resources.Resource))]
        public bool IsDownload2Mobile { get; set; }

        public HttpPostedFileBase File { get; set; }
    }
}
