using Models.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.GuardPatrol.JobManagement
{
    public class DetailTreeModel
    {
        public string RouteUniqueID { get; set; }

        public List<TreeItem> TreeItemList { get; set; }

        public string TreeData
        {
            get
            {
                return JsonConvert.SerializeObject(TreeItemList);
            }
        }

        public DetailTreeModel()
        {
            TreeItemList = new List<TreeItem>();
        }
    }
}
