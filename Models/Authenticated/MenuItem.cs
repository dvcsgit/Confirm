using System.Collections.Generic;

namespace Models.Authenticated
{
    public class MenuItem
    {
        public string ID { get; set; }

        public Dictionary<string, string> Description { get; set; }

        public string Area { get; set; }

        public string Controller { get; set; }

        public string Action { get; set; }

        public string Icon { get; set; }

        public int Seq { get; set; }

        public List<MenuItem> SubItemList { get; set; }

        public MenuItem()
        {
            Description = new Dictionary<string, string>();
            SubItemList = new List<MenuItem>();
        }
    }
}
