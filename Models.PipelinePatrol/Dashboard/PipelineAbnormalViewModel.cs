using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.Dashboard
{
    public class PipelineAbnormalViewModel
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public string Description { get; set; }

        public string AbnormalReasonUniqueID { get; set; }

        public string AbnormalReasonDescription { get; set; }

        public string AbnormalReasonRemark { get; set; }

        public string AbnormalReason
        {
            get
            {
                if (AbnormalReasonUniqueID == Define.OTHER)
                {
                    return AbnormalReasonRemark;
                }
                else
                {
                    if (!string.IsNullOrEmpty(AbnormalReasonDescription))
                    {
                        return AbnormalReasonDescription;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        public double LNG { get; set; }

        public double LAT { get; set; }
    }
}
