using Models.Shared;
using System.Collections.Generic;
using Utility;

namespace Models.EquipmentMaintenance.ProgressQuery
{
    public class ArriveRecordModel
    {
        public string UniqueID { get;set; }
        
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

        public List<string> PhotoList { get; set; }

        public bool HavePhoto
        {
            get
            {
                return this.PhotoList != null && this.PhotoList.Count > 0;
            }
        }

        public ArriveRecordModel()
        {
            PhotoList = new List<string>();
        }
    }
}
