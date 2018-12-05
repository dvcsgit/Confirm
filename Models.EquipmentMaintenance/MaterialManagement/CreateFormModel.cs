using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.MaterialManagement
{
    public class CreateFormModel
    {
        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }
        
        public List<SelectListItem> EquipmentTypeSelectItemList { get; set; }

        public List<SpecModel> SpecList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            EquipmentTypeSelectItemList = new List<SelectListItem>();
            SpecList = new List<SpecModel>();
        }
    }
}
