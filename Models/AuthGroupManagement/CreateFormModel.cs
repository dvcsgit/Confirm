using System.Collections.Generic;
namespace Models.AuthGroupManagement
{
    public class CreateFormModel
    {
        public string AncestorOrganizationUniqueID { get; set; }

        public FormInput FormInput { get; set; }

        public WebPermissionFunctionModel WebPermissionFunction { get; set; }

        public List<UserModel> UserList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            WebPermissionFunction = new WebPermissionFunctionModel();
            UserList = new List<UserModel>();
        }
    }
}
