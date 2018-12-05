using System;
using System.Linq;
using System.Collections.Generic;
using Utility;

namespace Report.EquipmentMaintenance.Models.CheckResultHour
{
    public class ControlPointModel
    {
        public string ControlPoint
        {
            get
            {
                if (!string.IsNullOrEmpty(ControlPointDescription))
                {
                    return string.Format("{0}/{1}", ControlPointID, ControlPointDescription);
                }
                else
                {
                    return ControlPointID;
                }
            }
        }

        public string ControlPointID { get; set; }

        public string ControlPointDescription { get; set; }

        public string ArriveTime
        {
            get
            {
                if (ArriveRecordList != null && ArriveRecordList.Count > 0)
                {
                    var arriveRecord = ArriveRecordList.OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).First();

                    return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(arriveRecord.ArriveDate, arriveRecord.ArriveTime));
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string BeginTime { get { return ArriveTime; } }

        public string EndTime
        {
            get
            {
                var checkResultList = ArriveRecordList.SelectMany(x => x.CheckResultList).ToList();

                if (checkResultList != null && checkResultList.Count > 0)
                {
                    var checkResult = checkResultList.OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).First();

                    return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(checkResult.CheckDate, checkResult.CheckTime));
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public List<ArriveRecordModel> ArriveRecordList { get; set; }

        public int TotalSeconds
        {
            get
            {
                int totalSeconds = 0;

                foreach (var arriveRecord in ArriveRecordList)
                {
                    var lastCheckResult = arriveRecord.CheckResultList.OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

                    if (lastCheckResult != null)
                    {
                        var beginTime = DateTimeHelper.DateTimeString2DateTime(arriveRecord.ArriveDate, arriveRecord.ArriveTime);
                        var endTime = DateTimeHelper.DateTimeString2DateTime(lastCheckResult.CheckDate, lastCheckResult.CheckTime);

                        if (beginTime.HasValue && endTime.HasValue)
                        {
                            totalSeconds += Convert.ToInt32((endTime.Value - beginTime.Value).TotalSeconds);
                        }
                    }
                }

                return totalSeconds;
            }
        }

        public string TimeSpan
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

        public ControlPointModel()
        {
            ArriveRecordList = new List<ArriveRecordModel>();
        }
    }
}
