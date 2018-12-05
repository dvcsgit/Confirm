using System.Collections.Generic;

namespace Models.AuthGroupManagement
{
    public class WebPermissionModel
    {
        public string ID { get; set; }

        public Dictionary<string, string> Description { get; set; }

        public List<WebFunctionModel> WebFunctionList { get; set; }

        public List<WebPermissionModel> SubItemList { get; set; }

        public WebPermissionModel()
        {
            Description = new Dictionary<string, string>();
            WebFunctionList = new List<WebFunctionModel>();
            SubItemList = new List<WebPermissionModel>();
        }
    }
}
