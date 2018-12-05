using Models.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.OrganizationManagement
{
    public class CreateFormModel
    {
        public string AncestorOrganizationUniqueID { get; set; }

        public string ParentUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<UserModel> UserList { get; set; }

        public List<EditableOrganizationModel> EditableOrganizationList { get; set; }

        public List<QueryableOrganizationModel> QueryableOrganizationList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            UserList = new List<UserModel>();
            EditableOrganizationList = new List<EditableOrganizationModel>();
            QueryableOrganizationList = new List<QueryableOrganizationModel>();
        }
    }
}
