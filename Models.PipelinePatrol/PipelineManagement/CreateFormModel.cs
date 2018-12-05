using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.PipelinePatrol.PipelineManagement
{
    public class CreateFormModel
    {
        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<SpecModel> SpecList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            SpecList = new List<SpecModel>();
        }
    }
}
