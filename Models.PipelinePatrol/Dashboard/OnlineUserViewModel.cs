using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.Dashboard
{
    public class OnlineUserViewModel
    {
        public string OrganizationDescription { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        //public string JobUniqueID { get; set; }

        //public string Job { get; set; }

        //public string RouteUniqueID { get; set; }

        //public string Route { get; set; }

        public double LNG { get; set; }

        public double LAT { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(UpdateTime);
            }
        }
    }
}
