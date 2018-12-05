using System;
using System.Collections.Generic;
using Utility;
using System.Linq;
using Models.Shared;
using System.Text;

namespace Models.EquipmentMaintenance.JobResultManagement
{
    public class JobResultModel
    {
        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string JobUniqueID { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public bool IsCycleEnd
        {
            get
            {
                DateTime endDate;

                if (!string.IsNullOrEmpty(EndTime))
                {
                    if (!string.IsNullOrEmpty(BeginTime))
                    {
                        if (string.Compare(BeginTime, EndTime) > 0)
                        {
                            endDate = DateTimeHelper.DateTimeString2DateTime(EndDate, EndTime).Value.AddDays(1);
                        }
                        else
                        {
                            endDate = DateTimeHelper.DateTimeString2DateTime(EndDate, EndTime).Value;
                        }
                    }
                    else
                    {
                        endDate = DateTimeHelper.DateTimeString2DateTime(EndDate, EndTime).Value;
                    }
                }
                else
                {
                    endDate = DateTimeHelper.DateString2DateTime(EndDate).Value.AddDays(1);
                }

                if (DateTime.Compare(DateTime.Now, endDate) >= 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

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

        public bool IsNeedVerify { get; set; }

        public int TimeMode { get; set; }

        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public DateTime JobBeginTime
        {
            get
            {
                return DateTimeHelper.DateTimeString2DateTime(BeginDate, BeginTime).Value;
            }
        }

        public DateTime JobEndTime
        {
            get
            {
                var jobEndTime = DateTimeHelper.DateTimeString2DateTime(EndDate, EndTime).Value;

                if (DateTime.Compare(JobBeginTime, jobEndTime) >= 0)
                {
                    jobEndTime = jobEndTime.AddDays(1);
                }

                return jobEndTime;
            }
        }

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

        public DateTime? LastCheckTime
        {
            get
            {
                var lastCheckResult = ControlPointList.SelectMany(x => x.CheckResultList).OrderByDescending(x => x.CheckDate).ThenByDescending(x => x.CheckDate).FirstOrDefault();

                if (lastCheckResult != null)
                {
                    return DateTimeHelper.DateTimeString2DateTime(lastCheckResult.CheckDate, lastCheckResult.CheckDate).Value;
                }
                else
                {
                    return null;
                }
            }
        }

        public List<ControlPointModel> ControlPointList { get; set; }

        public double CheckItemCount
        {
            get
            {
                return ControlPointList.Sum(x => x.CheckItemCount);
            }
        }

        public double CheckedItemCount
        {
            get
            {
                return ControlPointList.Sum(x => x.CheckedItemCount);
            }
        }

        public bool IsCompleted
        {
            get
            {
                return CheckedItemCount >= CheckItemCount;
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
                        if (IsCycleEnd)
                        {
                            return string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol);
                        }
                        else
                        {
                            return string.Format("{0}(0%)", Resources.Resource.Processing);
                        }
                    }
                    else
                    {
                        if (IsCompleted)
                        {
                            return Resources.Resource.Completed;
                        }
                        else
                        {
                            if (IsCycleEnd)
                            {
                                return string.Format("{0}({1})", Resources.Resource.Incomplete, (CheckedItemCount / CheckItemCount).ToString("#0.00%"));
                            }
                            else
                            {

                                return string.Format("{0}({1})", Resources.Resource.Processing, (CheckedItemCount / CheckItemCount).ToString("#0.00%"));
                            }
                        }
                    }
                }
            }
        }

        public string CompleteRateLabelClass
        {
            get
            {
                if (CheckItemCount == 0)
                {
                    return string.Empty;
                }
                else
                {
                    if (CheckedItemCount == 0)
                    {
                        if (IsCycleEnd)
                        {
                            return Define.Label_Color_Red_Class;
                        }
                        else
                        {
                            return Define.Label_Color_Blue_Class;
                        }
                    }
                    else
                    {
                        if (IsCompleted)
                        {
                            return Define.Label_Color_Green_Class;
                        }
                        else
                        {
                            if (IsCycleEnd)
                            {
                                return Define.Label_Color_Red_Class;
                            }
                            else
                            {

                                return Define.Label_Color_Blue_Class;
                            }
                        }
                    }
                }
            }
        }

        public string TimeSpan
        {
            get
            {
                var totalSeconds = ControlPointList.Sum(x => x.TotalSeconds);

                if (totalSeconds == 0)
                {
                    return "-";
                }
                else
                {
                    var ts = new TimeSpan(0, 0, totalSeconds);

                    return ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
                }
            }
        }

        public bool HaveAbnormal
        {
            get
            {
                return ControlPointList.Any(x => x.HaveAbnormal);
            }
        }

        public bool HaveAlert
        {
            get
            {
                return ControlPointList.Any(x => x.HaveAlert);
            }
        }

        //遲到
        //TimeMode == 0, 有設定開始時間, 第一點到位時間應該比開始時間早
        //TimeMode == 1, 有設定結束時間, 第一點到位時間應該比結束時間早
        public bool IsLate
        {
            get
            {
                if (TimeMode == 0 && !string.IsNullOrEmpty(BeginTime))
                {
                    if (FirstArriveTime.HasValue)
                    {
                        return DateTime.Compare(FirstArriveTime.Value, JobBeginTime) > 0;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (TimeMode == 1 && !string.IsNullOrEmpty(EndTime))
                {
                    if (FirstArriveTime.HasValue)
                    {
                        return DateTime.Compare(FirstArriveTime.Value, JobEndTime) > 0;
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

        //提早提開
        //TimeMode = 0, 有設定結束時間, 且路線已完成, 最後存檔時間應該比結束時間晚
        public bool IsLeaveEarly
        {
            get
            {
                if (TimeMode == 0 && !string.IsNullOrEmpty(EndTime) && IsCompleted)
                {
                    if (LastCheckTime.HasValue)
                    {
                        return DateTime.Compare(LastCheckTime.Value, JobEndTime) < 0;
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

        //提早到位
        //TimeMode = 1, 有設定開始時間, 第一點到位時間應該比開始時間晚
        public bool IsArriveEarly
        {
            get
            {
                if (TimeMode == 1 && !string.IsNullOrEmpty(BeginTime))
                {
                    if (FirstArriveTime.HasValue)
                    {
                        return DateTime.Compare(FirstArriveTime.Value, JobBeginTime) < 0;
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

        public string ArriveStatus
        {
            get
            {
                if (TimeMode == 0)
                {
                    if (IsLate && IsLeaveEarly)
                    {
                        return string.Format("{0} {1} {2}", Resources.Resource.Late, Resources.Resource.And, Resources.Resource.LeaveEarly);
                    }
                    else if (IsLate && !IsLeaveEarly)
                    {
                        return Resources.Resource.Late;
                    }
                    else if (!IsLate && IsLeaveEarly)
                    {
                        return Resources.Resource.LeaveEarly;
                    }
                    else
                    {
                        if (IsCompleted)
                        {
                            return Resources.Resource.Normal;
                        }
                        else
                        {
                            return "-";
                        }
                    }
                }
                else if (TimeMode == 1)
                {
                    if (IsLate)
                    {
                        return Resources.Resource.Late;
                    }
                    else if (IsArriveEarly)
                    {
                        return Resources.Resource.ArriveEarly;
                    }
                    else
                    {
                        if (IsCompleted)
                        {
                            return Resources.Resource.Normal;
                        }
                        else
                        {
                            return "-";
                        }
                    }
                }
                else
                {
                    if (IsCompleted)
                    {
                        return Resources.Resource.Normal;
                    }
                    else
                    {
                        return "-";
                    }
                }
            }
        }

        public string ArriveStatusLabelClass
        {
            get
            {
                if (TimeMode == 0)
                {
                    if (IsLate && IsLeaveEarly)
                    {
                        return Define.Label_Color_Red_Class;
                    }
                    else if (IsLate && !IsLeaveEarly)
                    {
                        return Define.Label_Color_Red_Class;
                    }
                    else if (!IsLate && IsLeaveEarly)
                    {
                        return Define.Label_Color_Red_Class;
                    }
                    else
                    {
                        if (IsCompleted)
                        {
                            return Define.Label_Color_Green_Class;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                }
                else if (TimeMode == 1)
                {
                    if (IsLate)
                    {
                        return Define.Label_Color_Red_Class;
                    }
                    else if (IsArriveEarly)
                    {
                        return Define.Label_Color_Red_Class;
                    }
                    else
                    {
                        if (IsCompleted)
                        {
                            return Define.Label_Color_Green_Class;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                }
                else
                {
                    if (IsCompleted)
                    {
                        return Define.Label_Color_Green_Class;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }

        public List<UserModel> JobUserList { get; set; }

        public string JobUsers
        {
            get
            {
                if (JobUserList != null && JobUserList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var jobUser in JobUserList)
                    {
                        sb.Append(jobUser.User);
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

        public List<UserModel> CheckUserList { get; set; }

        public string CheckUsers
        {
            get
            {
                if (CheckUserList != null && CheckUserList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var checkUser in CheckUserList)
                    {
                        sb.Append(checkUser.User);
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

        public JobResultModel()
        {
            ControlPointList = new List<ControlPointModel>();
            JobUserList = new List<UserModel>();
            CheckUserList = new List<UserModel>();
        }
    }
}
