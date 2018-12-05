using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Models.TankPatrol.HandlingMethodManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<SelectListItem> HandlingMethodTypeSelectItemList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            HandlingMethodTypeSelectItemList = new List<SelectListItem>();
        }
    }
}
