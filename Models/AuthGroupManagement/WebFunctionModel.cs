using System.Collections.Generic;

namespace Models.AuthGroupManagement
{
    public class WebFunctionModel
    {
        public string ID { get; set; }

        public Dictionary<string, string> Description { get; set; }

        public WebFunctionModel()
        {
            Description = new Dictionary<string, string>();
        }
    }
}
