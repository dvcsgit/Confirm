using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.GuardPatrol.ControlPointManagement
{
    public class CreateFormModel
    {
        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }
        
        public List<CheckItemModel> CheckItemList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
