using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.GuardPatrol.JobManagement
{
    public class CreateFormModel
    {
        public string AncestorOrganizationUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }
        
        public List<RouteModel> RouteList { get; set; }

        public List<UserModel> UserList { get; set; }

        public string BeginTimePickerValue
        {
            get
            {
                if (FormInput != null && !string.IsNullOrEmpty(FormInput.BeginTime))
                {
                    return FormInput.BeginTime.Substring(0, 2) + ":" + FormInput.BeginTime.Substring(2);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string EndTimePickerValue
        {
            get
            {
                if (FormInput != null && !string.IsNullOrEmpty(FormInput.EndTime))
                {
                    return FormInput.EndTime.Substring(0, 2) + ":" + FormInput.EndTime.Substring(2);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            RouteList = new List<RouteModel>();
            UserList = new List<UserModel>();
        }
    }
}
