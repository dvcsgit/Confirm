using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class LocationModel
    {
        public string UserID { get; set; }

        public DateTime DateTime { get; set; }

        public double LNG { get; set; }

        public double LAT { get; set; }

        //public string EquipmentPatrolRouteUniqueID { get; set; }

        public string JobUniqueID { get; set; }

        public string RouteUniqueID { get; set; }
    }
}
