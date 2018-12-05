using System.ComponentModel;

namespace Models.EquipmentMaintenance.ProgressQuery
{
    public class ControlPointExcelItem
    {
        public string JobDescription { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public string IsAbnormal { get; set; }

        public string ControlPoint { get; set; }

        public string CompleteRate { get; set; }

        public string TimeSpan { get; set; }

        public string ArriveDate { get; set; }

        public string ArriveTime { get; set; }

        public string User { get; set; }

        public string UnRFIDReason { get; set; }
    }
}
