using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Utility;

namespace Models.GuardPatrol.DailyReport
{
    public class JobRouteModel
    {
        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        //public string JobUniqueID { get; set; }

        //public string RouteUniqueID { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        //public bool IsOptional { get; set; }

        //public bool IsCycleEnd
        //{
        //    get
        //    {
        //        DateTime endDate;

        //        if (!string.IsNullOrEmpty(EndTime))
        //        {
        //            endDate = DateTimeHelper.DateTimeString2DateTime(EndDate, EndTime).Value;
        //        }
        //        else
        //        {
        //            endDate = DateTimeHelper.DateString2DateTime(EndDate).Value.AddDays(1);
        //        }

        //        if (DateTime.Compare(DateTime.Now, endDate) > 0)
        //        {
        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //}

        public string RouteID { get; set; }

        public string RouteName { get; set; }

        public string JobDescription { get; set; }

        public string Description
        {
            get
            {
                return string.Format("{0}/{1}-{2}", RouteID, RouteName, JobDescription);
            }
        }

        //public int TimeMode { get; set; }

        public string BeginTime { get; set; }

        //public string EndTime { get; set; }

        //public List<string> JobUserIDList { get; set; }

        //public List<string> JobUserList { get; set; }

        //public string JobUsers
        //{
        //    get
        //    {
        //        if (JobUserList.Count > 0)
        //        {
        //            var sb = new StringBuilder();

        //            foreach (var user in JobUserList)
        //            {
        //                sb.Append(user);

        //                sb.Append("、");
        //            }

        //            sb.Remove(sb.Length - 1, 1);

        //            return sb.ToString();
        //        }
        //        else
        //        {
        //            return string.Empty;
        //        }
        //    }
        //}

        public List<ControlPointModel> ControlPointList { get; set; }

        public List<string> CheckUserList
        {
            get
            {
                return ControlPointList.SelectMany(x => x.CheckUserList).Distinct().ToList();
            }
        }

        public string CheckUsers
        {
            get
            {
                if (CheckUserList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var user in CheckUserList)
                    {
                        sb.Append(user);

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

        //public double CheckItemCount
        //{
        //    get
        //    {
        //        return ControlPointList.Sum(x => x.CheckItemCount);
        //    }
        //}

        //public double CheckedItemCount
        //{
        //    get
        //    {
        //        return ControlPointList.Sum(x => x.CheckedItemCount);
        //    }
        //}

        //public bool IsComplete
        //{
        //    get
        //    {
        //        return CheckItemCount == CheckedItemCount;
        //    }
        //}

        //public string CompleteRate
        //{
        //    get
        //    {
        //        if (CheckItemCount == 0)
        //        {
        //            return "-";
        //        }
        //        else
        //        {
        //            if (CheckedItemCount == 0)
        //            {
        //                if (IsCycleEnd)
        //                {
        //                    if (IsOptional)
        //                    {
        //                        return Resources.Resource.UnPatrol;
        //                    }
        //                    else
        //                    {
        //                        return string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol);
        //                    }
        //                }
        //                else
        //                {
        //                    return string.Format("{0}(0%)", Resources.Resource.Processing);
        //                }
        //            }
        //            else
        //            {
        //                if (IsComplete)
        //                {
        //                    return Resources.Resource.Completed;
        //                }
        //                else
        //                {
        //                    if (IsCycleEnd)
        //                    {
        //                        return string.Format("{0}({1})", Resources.Resource.Incomplete, (CheckedItemCount / CheckItemCount).ToString("#0.00%"));
        //                    }
        //                    else
        //                    {
        //                        return string.Format("{0}({1})", Resources.Resource.Processing, (CheckedItemCount / CheckItemCount).ToString("#0.00%"));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public string LabelClass
        //{
        //    get
        //    {
        //        if (CheckItemCount == 0)
        //        {
        //            return string.Empty;
        //        }
        //        else
        //        {
        //            if (CheckedItemCount == 0)
        //            {
        //                if (IsCycleEnd)
        //                {
        //                    if (IsOptional)
        //                    {
        //                        return Define.Label_Color_Green_Class;
        //                    }
        //                    else
        //                    {
        //                        return Define.Label_Color_Red_Class;
        //                    }
        //                }
        //                else
        //                {
        //                    return Define.Label_Color_Blue_Class;
        //                }
        //            }
        //            else
        //            {
        //                if (IsComplete)
        //                {
        //                    return Define.Label_Color_Green_Class;
        //                }
        //                else
        //                {
        //                    if (IsCycleEnd)
        //                    {
        //                        if (IsOptional)
        //                        {
        //                            return Define.Label_Color_Green_Class;
        //                        }
        //                        else
        //                        {
        //                            return Define.Label_Color_Red_Class;
        //                        }
        //                    }
        //                    else
        //                    {

        //                        return Define.Label_Color_Blue_Class;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public string TimeSpan
        //{
        //    get
        //    {
        //        var totalSeconds = ControlPointList.Sum(x => x.TotalSeconds);

        //        if (totalSeconds == 0)
        //        {
        //            return "-";
        //        }
        //        else
        //        {
        //            var ts = new TimeSpan(0, 0, totalSeconds);

        //            return ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
        //        }
        //    }
        //}

        //public string UnPatrolReasonDescription { get; set; }

        //public string UnPatrolReasonRemark { get; set; }

        //public string UnPatrolReason
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(UnPatrolReasonDescription))
        //        {
        //            return UnPatrolReasonDescription;
        //        }
        //        else
        //        {
        //            return UnPatrolReasonRemark;
        //        }
        //    }
        //}

        //public bool HaveAbnormal
        //{
        //    get
        //    {
        //        return ControlPointList.Any(x => x.HaveAbnormal);
        //    }
        //}

        //public bool HaveAlert
        //{
        //    get
        //    {
        //        return ControlPointList.Any(x => x.HaveAlert);
        //    }
        //}

        //public DateTime JobBeginTime
        //{
        //    get
        //    {
        //        return DateTimeHelper.DateTimeString2DateTime(BeginDate, BeginTime).Value;
        //    }
        //}

        //public DateTime JobEndTime
        //{
        //    get
        //    {
        //        var jobEndTime = DateTimeHelper.DateTimeString2DateTime(EndDate, EndTime).Value;

        //        if (DateTime.Compare(JobBeginTime, jobEndTime) >= 0)
        //        {
        //            jobEndTime = jobEndTime.AddDays(1);
        //        }

        //        return jobEndTime;
        //    }
        //}

        public DateTime? FirstArriveTime
        {
            get
            {
                var firstArriveRecord = ControlPointList.SelectMany(x => x.ArriveRecordList).OrderBy(x => x.ArriveDate).ThenBy(x => x.ArriveTime).FirstOrDefault();

                if (firstArriveRecord != null)
                {
                    return DateTimeHelper.DateTimeString2DateTime(firstArriveRecord.ArriveDate, firstArriveRecord.ArriveTime).Value;
                }
                else
                {
                    return null;
                }
            }
        }

        //public DateTime? LastCheckTime
        //{
        //    get
        //    {
        //        var lastCheckResult = ControlPointList.SelectMany(x => x.CheckResultList).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckDate).FirstOrDefault();

        //        if (lastCheckResult != null)
        //        {
        //            return DateTimeHelper.DateTimeString2DateTime(lastCheckResult.CheckDate, lastCheckResult.CheckDate).Value;
        //        }
        //        else
        //        {
        //            return null;
        //        }
        //    }
        //}

        //public bool IsCompleted
        //{
        //    get
        //    {
        //        return CheckedItemCount >= CheckItemCount;
        //    }
        //}

        //遲到
        //TimeMode == 0, 有設定開始時間, 第一點到位時間應該比開始時間早
        //TimeMode == 1, 有設定結束時間, 第一點到位時間應該比結束時間早
        //public bool IsLate
        //{
        //    get
        //    {
        //        if (TimeMode == 0 && !string.IsNullOrEmpty(BeginTime))
        //        {
        //            if (FirstArriveTime.HasValue)
        //            {
        //                return DateTime.Compare(FirstArriveTime.Value, JobBeginTime) > 0;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        else if (TimeMode == 1 && !string.IsNullOrEmpty(EndTime))
        //        {
        //            if (FirstArriveTime.HasValue)
        //            {
        //                return DateTime.Compare(FirstArriveTime.Value, JobEndTime) > 0;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //}

        //提早提開
        //TimeMode = 0, 有設定結束時間, 且路線已完成, 最後存檔時間應該比結束時間晚
        //public bool IsLeaveEarly
        //{
        //    get
        //    {
        //        if (TimeMode == 0 && !string.IsNullOrEmpty(EndTime) && IsCompleted)
        //        {
        //            if (LastCheckTime.HasValue)
        //            {
        //                return DateTime.Compare(LastCheckTime.Value, JobEndTime) < 0;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //}

        //提早到位
        //TimeMode = 1, 有設定開始時間, 第一點到位時間應該比開始時間晚
        //public bool IsArriveEarly
        //{
        //    get
        //    {
        //        if (TimeMode == 1 && !string.IsNullOrEmpty(BeginTime))
        //        {
        //            if (FirstArriveTime.HasValue)
        //            {
        //                return DateTime.Compare(FirstArriveTime.Value, JobBeginTime) < 0;
        //            }
        //            else
        //            {
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //}

        //public string ArriveStatus
        //{
        //    get
        //    {
        //        if (TimeMode == 0)
        //        {
        //            if (IsLate && IsLeaveEarly)
        //            {
        //                return string.Format("{0} {1} {2}", Resources.Resource.Late, Resources.Resource.And, Resources.Resource.LeaveEarly);
        //            }
        //            else if (IsLate && !IsLeaveEarly)
        //            {
        //                return Resources.Resource.Late;
        //            }
        //            else if (!IsLate && IsLeaveEarly)
        //            {
        //                return Resources.Resource.LeaveEarly;
        //            }
        //            else
        //            {
        //                if (IsCompleted)
        //                {
        //                    return Resources.Resource.Normal;
        //                }
        //                else
        //                {
        //                    return "-";
        //                }
        //            }
        //        }
        //        else if (TimeMode == 1)
        //        {
        //            if (IsLate)
        //            {
        //                return Resources.Resource.Late;
        //            }
        //            else if (IsArriveEarly)
        //            {
        //                return Resources.Resource.ArriveEarly;
        //            }
        //            else
        //            {
        //                if (IsCompleted)
        //                {
        //                    return Resources.Resource.Normal;
        //                }
        //                else
        //                {
        //                    return "-";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (IsCompleted)
        //            {
        //                return Resources.Resource.Normal;
        //            }
        //            else
        //            {
        //                return "-";
        //            }
        //        }
        //    }
        //}

        //public string ArriveStatusLabelClass
        //{
        //    get
        //    {
        //        if (TimeMode == 0)
        //        {
        //            if (IsLate && IsLeaveEarly)
        //            {
        //                return Define.Label_Color_Red_Class;
        //            }
        //            else if (IsLate && !IsLeaveEarly)
        //            {
        //                return Define.Label_Color_Red_Class;
        //            }
        //            else if (!IsLate && IsLeaveEarly)
        //            {
        //                return Define.Label_Color_Red_Class;
        //            }
        //            else
        //            {
        //                if (IsCompleted)
        //                {
        //                    return Define.Label_Color_Green_Class;
        //                }
        //                else
        //                {
        //                    return string.Empty;
        //                }
        //            }
        //        }
        //        else if (TimeMode == 1)
        //        {
        //            if (IsLate)
        //            {
        //                return Define.Label_Color_Red_Class;
        //            }
        //            else if (IsArriveEarly)
        //            {
        //                return Define.Label_Color_Red_Class;
        //            }
        //            else
        //            {
        //                if (IsCompleted)
        //                {
        //                    return Define.Label_Color_Green_Class;
        //                }
        //                else
        //                {
        //                    return string.Empty;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (IsCompleted)
        //            {
        //                return Define.Label_Color_Green_Class;
        //            }
        //            else
        //            {
        //                return string.Empty;
        //            }
        //        }
        //    }
        //}

        //public string OverTimeReasonDescription { get; set; }

        //public string OverTimeReasonRemark { get; set; }

        //public string OverTimeReason
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(OverTimeReasonDescription))
        //        {
        //            return OverTimeReasonDescription;
        //        }
        //        else
        //        {
        //            return OverTimeReasonRemark;
        //        }
        //    }
        //}

        public JobRouteModel()
        {
            //JobUserIDList = new List<string>();
            //JobUserList = new List<string>();
            ControlPointList = new List<ControlPointModel>();
        }
    }
}
