using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.TankPatrol.AbnormalReasonManagement
{
    public class CreateFormModel
    {
        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<SelectListItem> AbnormalTypeSelectItemList { get; set; }

        public List<HandlingMethodModel> HandlingMethodList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            AbnormalTypeSelectItemList = new List<SelectListItem>();
            HandlingMethodList = new List<HandlingMethodModel>();
        }
    }
}
