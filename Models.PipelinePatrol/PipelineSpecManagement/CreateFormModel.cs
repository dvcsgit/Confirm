using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.PipelinePatrol.PipelineSpecManagement
{
    public class CreateFormModel
    {
        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }
        
        public List<SelectListItem> TypeSelectItemList { get; set; }

        public List<OptionModel> OptionList { get; set; }
        
        public CreateFormModel()
        {
            FormInput = new FormInput();
            TypeSelectItemList = new List<SelectListItem>();
            OptionList = new List<OptionModel>();
        }
    }
}
