using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TankPatrol.DataSync
{
    public class IslandModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public List<PortModel> PortList { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return PortList.SelectMany(x => x.CheckItemList).ToList();
            }
        }

        public IslandModel()
        {
            PortList = new List<PortModel>();
        }
    }
}
