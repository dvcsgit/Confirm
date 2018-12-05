using System;
using System.Linq;
using System.Collections.Generic;
using Utility;

namespace Models.EquipmentMaintenance.ProgressQuery
{
    public class ControlPointModel
    {
        public string JobResultUniqueID { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public string ControlPoint
        {
            get
            {
                return string.Format("{0}/{1}", ID, Description);
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

        public List<string> CheckUserList
        {
            get
            {
                return ArriveRecordList.Select(x => x.User.User).Distinct().ToList();
            }
        }

        public List<CheckItemModel> CheckItemList { get; set; }

        public List<CheckResultModel> CheckResultList
        {
            get
            {
                return CheckItemList.SelectMany(x => x.CheckResultList).ToList();
            }
        }

        public bool HaveAbnormal
        {
            get
            {
                return CheckItemList.Any(x => x.HaveAbnormal);
            }
        }

        public bool HaveAlert
        {
            get
            {
                return CheckItemList.Any(x => x.HaveAlert);
            }
        }

        public double CheckItemCount
        {
            get
            {
                return CheckItemList.Count;
            }
        }

        public double CheckedItemCount
        {
            get
            {
                return CheckItemList.Count(x => x.IsChecked);
            }
        }

        public string CompleteRate
        {
            get
            {
                if (CheckItemCount == 0)
                {
                    return "-";
                }
                else
                {
                    if (CheckedItemCount == 0)
                    {
                        return "0%";
                    }
                    else
                    {

                        if (CheckItemCount == CheckedItemCount)
                        {
                            return "100%";
                        }
                        else
                        {
                            return (CheckedItemCount / CheckItemCount).ToString("#0.00%");
                        }
                    }
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
