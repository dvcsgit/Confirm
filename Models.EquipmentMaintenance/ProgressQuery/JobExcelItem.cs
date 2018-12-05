using System.ComponentModel;

namespace Models.EquipmentMaintenance.ProgressQuery
{
    public class JobExcelItem
    {
        public string IsAbnormal { get; set; }

        public string JobDescription { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public string CompleteRate { get; set; }

        public string ExecBeginDateTimeString { get; set; }

        public string ExecEndDateTimeString { get; set; }

        public string ExecTimeSpan { get; set; }

        public string TimeSpan { get; set; }

        public string User { get; set; }

        public string UnPatrolReason { get; set; }

        public string ArriveStatus { get; set; }

        public string OverTimeReason { get; set; }
    }
}
