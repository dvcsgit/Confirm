using System.Linq;
using System.Collections.Generic;

namespace Models.TankPatrol.DataSync
{
    public class PortModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public string TagID { get; set; }

        public int? LB2LPTimeSpan { get; set; }

        public int? LP2LATimeSpan { get; set; }

        public int? LA2LDTimeSpan { get; set; }

        public int? UB2UPTimeSpan { get; set; }

        public int? UP2UATimeSpan { get; set; }

        public int? UA2UDTimeSpan { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public PortModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
