using System.ComponentModel.DataAnnotations;

namespace Models.EmgContactManagement
{
    public class CreateFormModel
    {
        public string AncestorOrganizationUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }
        
        public CreateFormModel()
        {
            FormInput = new FormInput();
        }
    }
}
