using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.EmgContactManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string AncestorOrganizationUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public string UserName { get; set; }

        public string User
        {
            get
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    return string.Format("{0}/{1}", FormInput.UserID, UserName);
                }
                else
                {
                    return FormInput.UserID;
                }
            }
        }

        public FormInput FormInput { get; set; }

        public List<string> TelList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            TelList = new List<string>();
        }
    }
}
