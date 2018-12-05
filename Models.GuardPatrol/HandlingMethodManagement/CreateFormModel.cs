using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.GuardPatrol.HandlingMethodManagement
{
    public class CreateFormModel
    {
        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<SelectListItem> HandlingMethodTypeSelectItemList { get; set; }

        public CreateFormModel()
        {
            this.FormInput = new FormInput();
            this.HandlingMethodTypeSelectItemList = new List<SelectListItem>();
        }
    }
}
