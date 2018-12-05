using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Models.PipelinePatrol.AbnormalReasonManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<SelectListItem> AbnormalTypeSelectItemList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            AbnormalTypeSelectItemList = new List<SelectListItem>();
        }
    }
}
