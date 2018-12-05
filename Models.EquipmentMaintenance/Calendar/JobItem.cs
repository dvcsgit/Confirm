using System;
using Utility;

namespace Models.EquipmentMaintenance.Calendar
{
    public class JobItem
    {
        public string JobResultUniqueID { get; set; }

        public string JobDescription { get; set; }

        public string Display
        {
            get
            {
                return string.Format("【巡檢作業】{0}({1})", JobDescription, CompleteRate);
            }
        }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public string Color
        {
            get
            {
                if (CompleteRate == string.Format("{0} {1}", Resources.Resource.Job, Resources.Resource.UnPatrol))
                {
                    return Define.Color_Red;
                }
                else if (CompleteRate == Resources.Resource.Completed)
                {
                    return Define.Color_Green;
                }
                else if (CompleteRate.StartsWith(Resources.Resource.Processing))
                {
                    DateTime? begin;

                    if (!string.IsNullOrEmpty(BeginTime))
                    {
                        begin = new DateTime(BeginDate.Year, BeginDate.Month, BeginDate.Day, int.Parse(BeginTime.Substring(0, 2)), int.Parse(BeginTime.Substring(2, 2)), 0);
                    }
                    else
                    {
                        begin = BeginDate;
                    }

                    if (DateTime.Compare(DateTime.Now, begin.Value) >= 0)
                    {
                        return Define.Color_Blue;
                    }
                    else
                    {
                        CompleteRate = Resources.Resource.JobDateYet;

                        return Define.Color_Gray;
                    }
                }
                else
                {
                    return Define.Color_Orange;
                }
            }
        }

        public string CompleteRate { get; set; }

        public string Begin
        {
            get
            {
                if (!string.IsNullOrEmpty(BeginTime))
                {
                    return DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(BeginDate), BeginTime).Value.ToString("s");
                }
                else
                {
                    return this.BeginDate.ToString("s");
                }
            }
        }

        public string End
        {
            get
            {
                if (!string.IsNullOrEmpty(EndTime))
                {
                    var begin = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(BeginDate), BeginTime);
                    var end = DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateTime2DateString(EndDate), EndTime);

                    if (DateTime.Compare(end.Value, begin.Value) < 0)
                    {
                        end = end.Value.AddDays(1);
                    }

                    return end.Value.ToString("s");
                }
                else
                {
                    return this.EndDate.ToString("s");
                }
            }
        }

        public bool IsAllDay
        {
            get
            {
                return string.IsNullOrEmpty(BeginTime) && string.IsNullOrEmpty(EndTime);
            }
        }
    }
}
