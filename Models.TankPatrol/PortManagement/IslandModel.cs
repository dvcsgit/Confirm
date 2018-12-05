using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TankPatrol.PortManagement
{
    public class IslandModel
    {
        public string StationUniqueID { get; set; }

        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public string Display
        {
            get
            {
                return string.Format("{0}/{1}", ID, Description);
            }
        }
    }
}
