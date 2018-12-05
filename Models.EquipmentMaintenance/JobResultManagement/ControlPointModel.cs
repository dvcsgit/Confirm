using System;
using System.Collections.Generic;
using System.Linq;
using Utility;

namespace Models.EquipmentMaintenance.JobResultManagement
{
    public class ControlPointModel
    {
        public string UniqueID { get; set; }

        public List<ArriveRecordModel> ArriveRecordList { get; set; }

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

        public ControlPointModel()
        {
            ArriveRecordList = new List<ArriveRecordModel>();
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
