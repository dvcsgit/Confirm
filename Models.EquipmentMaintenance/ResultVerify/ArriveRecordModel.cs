using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.ResultVerify
{
    public class ArriveRecordModel
    {
        public string UniqueID { get; set; }

        public string ArriveDate { get; set; }

        public string ArriveTime { get; set; }

        public UserModel User { get; set; }

        public string UnRFIDReasonID { get; set; }

        public string UnRFIDReasonDescription { get; set; }

        public string UnRFIDReasonRemark { get; set; }

        public string UnRFIDReason
        {
            get
            {
                if (UnRFIDReasonID == Define.OTHER)
                {
                    return UnRFIDReasonRemark;
                }
                else
                {
                    return UnRFIDReasonDescription;
                }
            }
        }

        public string TimeSpanAbnormalReasonID { get; set; }

        public string TimeSpanAbnormalReasonDescription { get; set; }

        public string TimeSpanAbnormalReasonRemark { get; set; }

        public string TimeSpanAbnormalReason
        {
            get
            {
                if (TimeSpanAbnormalReasonID == Define.OTHER)
                {
                    return TimeSpanAbnormalReasonRemark;
                }
                else
                {
                    return TimeSpanAbnormalReasonDescription;
                }
            }
        }
    }
}
