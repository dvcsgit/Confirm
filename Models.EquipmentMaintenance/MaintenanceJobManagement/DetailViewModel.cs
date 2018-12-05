using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceJobManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "MaintenanceJobDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        public string CycleMode { get; set; }

        public int CycleCount { get; set; }

        public string DayMode { get; set; }
        public string WeekMode { get; set; }
        public string MonthMode { get; set; }
        public bool Mon { get; set; }
        public bool Tue { get; set; }
        public bool Wed { get; set; }
        public bool Thu { get; set; }
        public bool Fri { get; set; }
        public bool Sat { get; set; }
        public bool Sun { get; set; }
        public int? Day { get; set; }

        public string CycleDisplay
        {
            get
            {
                if (CycleMode == "D")
                {
                    if (DayMode == "H")
                    {
                        return string.Format("{0}{1}{2}({3})", Resources.Resource.Every, CycleCount, Resources.Resource.Day, "假日");
                    }
                    else if (DayMode == "W")
                    {
                        return string.Format("{0}{1}{2}({3})", Resources.Resource.Every, CycleCount, Resources.Resource.Day, "非假日");
                    }
                    else
                    {
                        return string.Format("{0}{1}{2}", Resources.Resource.Every, CycleCount, Resources.Resource.Day);
                    }
                }
                else if (CycleMode == "W")
                {
                    if (WeekMode == "I")
                    {
                        return string.Format("{0}{1}{2}", Resources.Resource.Every, CycleCount, Resources.Resource.Week);
                    }
                    else
                    {
                        var sb = new StringBuilder();

                        if (Mon)
                        {
                            sb.Append("星期一");
                            sb.Append("、");
                        }
                        if (Tue)
                        {
                            sb.Append("星期二");
                            sb.Append("、");
                        }
                        if (Wed)
                        {
                            sb.Append("星期三");
                            sb.Append("、");
                        }
                        if (Thu)
                        {
                            sb.Append("星期四");
                            sb.Append("、");
                        }
                        if (Fri)
                        {
                            sb.Append("星期五");
                            sb.Append("、");
                        }
                        if (Sat)
                        {
                            sb.Append("星期六");
                            sb.Append("、");
                        }
                        if (Sun)
                        {
                            sb.Append("星期日");
                            sb.Append("、");
                        }

                        if (sb.Length > 1)
                        {
                            sb.Remove(sb.Length - 1, 1);
                        }

                        if (sb.Length > 0)
                        {
                            return string.Format("{0}{1}{2}({3})", Resources.Resource.Every, CycleCount, Resources.Resource.Week, sb.ToString());
                        }
                        else
                        {
                            return string.Format("{0}{1}{2}", Resources.Resource.Every, CycleCount, Resources.Resource.Week);
                        }
                    }
                }
                else if (CycleMode == "M")
                {
                    if (MonthMode == "I")
                    {
                        return string.Format("{0}{1}{2}", Resources.Resource.Every, CycleCount, Resources.Resource.Month);
                    }
                    else
                    {
                        return string.Format("{0}{1}{2}({3}日)", Resources.Resource.Every, CycleCount, Resources.Resource.Month, Day);
                    }
                }
                else if (CycleMode == "Y")
                {
                    return string.Format("{0}{1}{2}", Resources.Resource.Every, CycleCount, Resources.Resource.Year);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [Display(Name = "NotifyDay", ResourceType = typeof(Resources.Resource))]
        public int NotifyDay { get; set; }

        [Display(Name = "BeginDate", ResourceType = typeof(Resources.Resource))]
        public string BeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(this.BeginDate);
            }
        }

        public DateTime BeginDate { get; set; }

        [Display(Name = "EndDate", ResourceType = typeof(Resources.Resource))]
        public string EndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(this.EndDate);
            }
        }

        public DateTime? EndDate { get; set; }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }

        public List<UserModel> UserList { get; set; }

        public DetailViewModel()
        {
            UserList = new List<UserModel>();
        }
    }
}
