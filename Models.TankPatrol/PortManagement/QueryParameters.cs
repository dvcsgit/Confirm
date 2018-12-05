using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TankPatrol.PortManagement
{
    public class QueryParameters
    {
        public string OrganizationUniqueID { get; set; }

        public string StationUniqueID { get; set; }

        public string IslandUniqueID { get; set; }

        public string Keyword { get; set; }
    }
}
