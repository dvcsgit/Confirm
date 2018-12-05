using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class InspectionFormInput
    {
        public string PipePointUniqueID { get; set; }

        public string ConstructionFirmUniqueID { get; set; }

        public string ConstructionFirmRemark { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public string ConstructionTypeUniqueID { get; set; }

        public string ConstructionTypeRemark { get; set; }

        public double LNG { get; set; }

        public double LAT { get; set; }

        public string Address { get; set; }

        public string UserID { get; set; }

        public string Description { get; set; }
    }
}
