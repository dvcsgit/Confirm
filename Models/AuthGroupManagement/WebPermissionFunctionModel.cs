using System.Collections.Generic;

namespace Models.AuthGroupManagement
{
    public class WebPermissionFunctionModel
    {
        public List<WebPermissionModel> WebPermissionList { get; set; }

        public List<AuthGroupWebPermissionFunction> AuthGroupWebPermissionFunctionList { get; set; }

        public WebPermissionFunctionModel()
        {
            WebPermissionList = new List<WebPermissionModel>();
            AuthGroupWebPermissionFunctionList = new List<AuthGroupWebPermissionFunction>();
        }
    }
}
