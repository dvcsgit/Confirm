using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.DataSync
{
    public class PipelineAbnormalFormInput
    {
        public string PipePointUniqueID { get; set; }

        public double LNG { get; set; }

        public double LAT { get; set; }

        public string Address { get; set; }

        public string UserID { get; set; }

        public string Description { get; set; }

        public string AbnormalReasonUniqueID { get; set; }

        public string AbnormalReasonRemark { get; set; }
    }
}
