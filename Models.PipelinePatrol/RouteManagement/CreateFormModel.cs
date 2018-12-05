using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.PipelinePatrol.RouteManagement
{
    public class CreateFormModel
    {
        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }
        
        public List<CheckPointModel> CheckPointList { get; set; }

        public List<PipelineModel> PipelineList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            CheckPointList = new List<CheckPointModel>();
            PipelineList = new List<PipelineModel>();
        }
    }
}
