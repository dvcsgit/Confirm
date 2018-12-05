using System;
using Utility;
namespace Models.GuardPatrol.JobManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationDescription { get; set; }

        public string Description { get; set; }

        public DateTime BeginDate { get; set; }

        public string BeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(BeginDate);
            }
        }

        public DateTime? EndDate { get; set; }

        public string EndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EndDate);
            }
        }

        public string BeginTime { get; set; }

        public string BeginTimeString
        {
            get
            {
                if (!string.IsNullOrEmpty(BeginTime))
                {
                    return string.Format("{0}:{1}", BeginTime.Substring(0, 2), BeginTime.Substring(2));
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string EndTime { get; set; }

        public string EndTimeString
        {
            get
            {
                if (!string.IsNullOrEmpty(EndTime))
                {
                    return string.Format("{0}:{1}", EndTime.Substring(0, 2), EndTime.Substring(2));
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public int CycleCount { get; set; }

        public string CycleMode { get; set; }

        public string Cycle
        {
            get
            {
                var cycleMode = string.Empty;

                if (CycleMode == "D")
                {
                    cycleMode = Resources.Resource.Day;
                }
                else if (CycleMode == "W")
                {
                    cycleMode = Resources.Resource.Week;
                }
                else if (CycleMode == "M")
                {
                    cycleMode = Resources.Resource.Month;
                }
                else if (CycleMode == "Y")
                {
                    cycleMode = Resources.Resource.Year;
                }

                return string.Format("{0}{1}{2}", Resources.Resource.Every, CycleCount, cycleMode);
            }
        }

        public GridItem()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
