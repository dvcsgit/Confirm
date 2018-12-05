using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.ResultVerify
{
    public class ControlPointModel
    {
        public string ID { get; set; }

        public string Description { get; set; }

        public string ControlPoint
        {
            get
            {
                return string.Format("{0}/{1}", ID, Description);
            }
        }

        public bool IsUnRFID
        {
            get
            {
                return ArriveRecordList.Any(x => !string.IsNullOrEmpty(x.UnRFIDReason));
            }
        }

        public string TimeSpanAbnormalReason
        {
            get
            {
                var recordList = ArriveRecordList.Where(x => !string.IsNullOrEmpty(x.TimeSpanAbnormalReason)).ToList();

                if (recordList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var record in recordList)
                    {
                        if (!string.IsNullOrEmpty(record.TimeSpanAbnormalReason))
                        {
                            sb.Append(record.TimeSpanAbnormalReason);
                            sb.Append("、");
                        }
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string CheckUsers
        {
            get
            {
                if (ArriveRecordList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var record in ArriveRecordList)
                    {
                        sb.Append(record.User.User);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public DateTime? ArriveDateTime
        {
            get
            {
                if (ArriveRecordList != null && ArriveRecordList.Count > 0)
                {
                    var q = ArriveRecordList.OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).First();

                    return DateTimeHelper.DateTimeString2DateTime(q.ArriveDate, q.ArriveTime);
                }
                else
                {
                    return null;
                }
            }
        }

        public DateTime? FinishedDateTime
        {
            get
            {
                if (CheckResultList != null && CheckResultList.Count > 0)
                {
                    var q = CheckResultList.OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).First();

                    return DateTimeHelper.DateTimeString2DateTime(q.CheckDate, q.CheckTime);
                }
                else
                {
                    return null;
                }
            }
        }

        public List<ArriveRecordModel> ArriveRecordList { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public List<CheckResultModel> CheckResultList
        {
            get
            {
                return CheckItemList.SelectMany(x => x.CheckResultList).ToList();
            }
        }

        public List<CheckItemModel> AbnormalList
        {
            get
            {
                return CheckItemList.Where(x => x.IsAbnormal || x.IsAlert).ToList();
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

        public int TotalSeconds
        {
            get
            {
                int totalSeconds = 0;

                foreach (var arriveRecord in ArriveRecordList)
                {
                    var lastCheckResult = CheckResultList.Where(x => x.ArriveRecordUniqueID == arriveRecord.UniqueID).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckTime).FirstOrDefault();

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

        public int? MinTimeSpan { get; set; }

        public bool IsTimeSpanAbnormal
        {
            get
            {
                if (MinTimeSpan.HasValue)
                {
                    var minSeconds = MinTimeSpan.Value * 60;

                    if (TotalSeconds < minSeconds)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public ControlPointModel()
        {
            ArriveRecordList = new List<ArriveRecordModel>();
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
