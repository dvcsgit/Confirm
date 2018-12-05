using Models.Shared;
using System.Collections.Generic;
using Utility;

namespace Customized.CHIMEI.Models.AIMSJobQuery
{
    public class ArriveRecordModel
    {
        public string ArriveDate { get; set; }

        public string ArriveTime { get; set; }

        public string ArriveDateTime
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(ArriveDate, ArriveTime));
            }
        }

        public UserModel ArriveUser { get; set; }

        public string UnRFIDReasonDescription { get; set; }

        public string UnRFIDReasonRemark { get; set; }

        public string UnRFIDReason
        {
            get
            {
                if (!string.IsNullOrEmpty(UnRFIDReasonDescription))
                {
                    return UnRFIDReasonDescription;
                }
                else
                {
                    return UnRFIDReasonRemark;
                }
            }
        }

        public ArriveRecordModel()
        {
            ArriveUser = new UserModel();
        }
    }
}
