using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.ControlPointManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }
        
        public List<CheckItemModel> CheckItemList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
