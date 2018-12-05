using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.MaintenanceJobManagement
{
    public class CreateFormModel
    {
        public string AncestorOrganizationUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<EquipmentModel> EquipmentList { get; set; }

        public List<UserModel> UserList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            EquipmentList = new List<EquipmentModel>();
            UserList = new List<UserModel>();
        }
    }
}
