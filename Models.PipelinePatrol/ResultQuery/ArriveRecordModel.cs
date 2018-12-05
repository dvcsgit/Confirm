using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.ResultQuery
{
    public class ArriveRecordModel
    {
        public string UniqueID { get; set; }

        public string ArriveDate { get; set; }

        public string ArriveTime { get; set; }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public string User
        {
            get
            {
                return string.Format("{0}/{1}", UserID, UserName);
            }
        }

        public double? LNG { get; set; }

        public double? LAT { get; set; }

        public string TimeSpanAbnormalReasonDescription { get; set; }

        public string TimeSpanAbnormalReasonRemark { get; set; }

        public string TimeSpanAbnormalReason
        {
            get
            {
                if (!string.IsNullOrEmpty(TimeSpanAbnormalReasonDescription))
                {
                    return TimeSpanAbnormalReasonDescription;
                }
                else
                {
                    return TimeSpanAbnormalReasonRemark;
                }
            }
        }
    }
}
