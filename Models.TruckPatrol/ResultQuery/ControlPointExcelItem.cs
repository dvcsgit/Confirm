using System.ComponentModel;

namespace Models.TruckPatrol.ResultQuery
{
    public class ControlPointExcelItem
    {
        [DisplayName("異常")]
        public string Abnormal { get; set; }
        [DisplayName("車頭")]
        public string CarNo { get; set; }
        [DisplayName("尾車")]
        public string SecondCarNo { get; set; }
        [DisplayName("管制點")]
        public string ControlPoint { get; set; }
        [DisplayName("完成率")]
        public string CompleteRate { get; set; }
        [DisplayName("作業時間")]
        public string TimeSpan { get; set; }
        [DisplayName("到位日期")]
        public string ArriveDate { get; set; }
        [DisplayName("到位時間")]
        public string ArriveTime { get; set; }
        [DisplayName("到位人員")]
        public string User { get; set; }
        [DisplayName("管制點未感應RFID原因")]
        public string UnRFIDReason { get; set; }
    }
}
