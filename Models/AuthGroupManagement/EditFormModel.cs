using System.Collections.Generic;
namespace Models.AuthGroupManagement
{
    public class EditFormModel
    {

        public string AncestorOrganizationUniqueID { get; set; }
        public string AuthGroupID { get; set; }

        public FormInput FormInput { get; set; }

        public WebPermissionFunctionModel WebPermissionFunction { get; set; }

        public List<UserModel> UserList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            WebPermissionFunction = new WebPermissionFunctionModel();
            UserList = new List<UserModel>();
        }
    }
}
