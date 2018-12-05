using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.GuardPatrol.JobManagement
{
    public class RouteModel
    {
        public string UniqueID { get; set; }

        public bool IsOptional { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public List<ControlPointModel> ControlPointList { get; set; }

        public RouteModel()
        {
            ControlPointList = new List<ControlPointModel>();
        }
    }
}
