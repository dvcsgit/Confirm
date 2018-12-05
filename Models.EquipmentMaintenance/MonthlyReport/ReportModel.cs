using System;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.MonthlyReport
{
    public class ReportModel
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public int DaysInMonth
        {
            get
            {
                return DateTime.DaysInMonth(Year, Month);
            }
        }

        public DateTime BeginDate
        {
            get
            {
                return new DateTime(Year, Month, 1);
            }
        }

        public DateTime EndDate
        {
            get
            {
                return new DateTime(Year, Month, DaysInMonth);
            }
        }

        public string Ym
        {
            get
            {
                return string.Format("{0}{1}{2}{3}", Year.ToString().PadLeft(4, '0'), Resources.Resource.Year, Month.ToString().PadLeft(2, '0'), Resources.Resource.Month);
            }
        }

        public string OrganizationDescription { get; set; }

        public string RouteName { get; set; }

        public List<JobModel> JobList { get; set; }

        public List<CheckItemModel> ItemList { get; set; }

        public ReportModel()
        {
            JobList = new List<JobModel>();
            ItemList = new List<CheckItemModel>();
        }
    }
}
