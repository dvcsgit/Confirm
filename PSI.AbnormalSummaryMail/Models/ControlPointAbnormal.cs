using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace PSI.AbnormalSummaryMail.Models
{
    public class ControlPointAbnormal
    {
        public string OrganizationDescription { get; set; }

        public string RouteName { get; set; }

        public string JobDescription { get; set; }

        public string ControlPointID { get; set; }

        public string ControlPointDescription { get; set; }

        public string ControlPoint
        {
            get
            {
                return string.Format("{0}/{1}", ControlPointID, ControlPointDescription);
            }
        }

        public string ArriveDate { get; set; }

        public string ArriveTime { get; set; }

        public string ArriveDateTime
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(ArriveDate, ArriveTime));
            }
        }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public string User
        {
            get
            {
                return string.Format("{0}/{1}", UserID, UserName);
            }
        }

        public string UnRFIDReasonUniqueID { get; set; }

        public string UnRFIDReasonDescription { get; set; }

        public string UnRFIDReasonRemark { get; set; }

        public string UnRFIDReason
        {
            get
            {
                if (UnRFIDReasonUniqueID == Define.OTHER)
                {
                    return UnRFIDReasonRemark;
                }
                else
                {
                    return UnRFIDReasonDescription;
                }
            }
        }
    }
}
