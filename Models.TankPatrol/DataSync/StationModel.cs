using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TankPatrol.DataSync
{
    public class StationModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public List<IslandModel> IslandList { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return IslandList.SelectMany(x => x.CheckItemList).ToList();
            }
        }

        public StationModel()
        {
            IslandList = new List<IslandModel>();
        }
    }
}
