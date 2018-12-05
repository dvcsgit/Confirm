using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.AuthGroupManagement
{
    public class DetailViewModel
    {
        [Display(Name = "AuthGroupID", ResourceType = typeof(Resources.Resource))]
        public string AuthGroupID { get; set; }

        [Display(Name = "AuthGroupName", ResourceType = typeof(Resources.Resource))]
        public string AuthGroupName { get; set; }

        public WebPermissionFunctionModel WebPermissionFunction { get; set; }

        public List<UserModel> UserList { get; set; }

        public DetailViewModel()
        {
            WebPermissionFunction = new WebPermissionFunctionModel();
            UserList = new List<UserModel>();
        }
    }
}
