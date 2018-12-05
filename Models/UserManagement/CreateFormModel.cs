using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.UserManagement
{
    public class CreateFormModel
    {
        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }
        
        public List<AuthGroup> AuthGroupList { get; set; }

        public List<string> UserAuthGroupList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            AuthGroupList = new List<AuthGroup>();
            UserAuthGroupList = new List<string>();
        }
    }
}
