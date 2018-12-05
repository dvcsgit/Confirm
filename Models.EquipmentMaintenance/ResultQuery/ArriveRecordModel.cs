using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.ResultQuery
{
    public class ArriveRecordModel
    {
        public string JobUniqueID { get; set; }

        public string CheckDate { get; set; }

        public string UniqueID { get; set; }

        public string ControlPointID { get; set; }

        public string ControlPointDescription { get; set; }

        public string ControlPoint
        {
            get
            {
                return string.Format("{0}/{1}", ControlPointID, ControlPointDescription);
            }
        }

        public string Date { get; set; }

        public string Time { get; set; }

        public string ArriveTime
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(Date, Time).Value);
            }
        }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public string User
        {
            get
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    return string.Format("{0}/{1}", UserID, UserName);
                }
                else
                {
                    return UserID;
                }
            }
        }

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

        public double? MinTimeSpan { get; set; }

        public int TotalSeconds
        {
            get
            {
                int totalSeconds = 0;

                var lastCheckResult = CheckResultList.OrderByDescending(x => x.Date).ThenByDescending(x => x.Time).FirstOrDefault();

                if (lastCheckResult != null)
                {
                    var beginTime = DateTimeHelper.DateTimeString2DateTime(Date, Time);
                    var endTime = DateTimeHelper.DateTimeString2DateTime(lastCheckResult.Date, lastCheckResult.Time);

                    if (beginTime.HasValue && endTime.HasValue)
                    {
                        totalSeconds += Convert.ToInt32((endTime.Value - beginTime.Value).TotalSeconds);
                    }
                }

                return totalSeconds;
            }
        }

        public string TotalTimeSpan
        {
            get
            {
                if (TotalSeconds == 0)
                {
                    return "-";
                }
                else
                {
                    var ts = new TimeSpan(0, 0, TotalSeconds);

                    return ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
                }
            }
        }

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

        public bool HaveAbnormal
        {
            get
            {
                return CheckResultList.Any(x => x.IsAbnormal);
            }
        }

        public bool HaveAlert
        {
            get
            {
                return CheckResultList.Any(x => x.IsAlert);
            }
        }

        public List<CheckResultModel> CheckResultList { get; set; }

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
            CheckResultList = new List<CheckResultModel>();
        }
    }
}
