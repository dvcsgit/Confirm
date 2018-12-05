using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.MaintenanceJobManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public string AncestorOrganizationUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        public FormInput FormInput { get; set; }

        public List<EquipmentModel> EquipmentList { get; set; }

        public List<UserModel> UserList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            EquipmentList = new List<EquipmentModel>();
            UserList = new List<UserModel>();
        }
    }
}
