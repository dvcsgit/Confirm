using System.ComponentModel;

namespace Models.TruckPatrol.ResultQuery
{
    public class CheckItemExcelItem
    {
        [DisplayName("異常")]
        public string Abnormal { get; set; }
        [DisplayName("車頭")]
        public string CarNo { get; set; }
        [DisplayName("尾車")]
        public string SecondCarNo { get; set; }
        [DisplayName("管制點")]
        public string ControlPoint { get; set; }
        [DisplayName("檢查項目")]
        public string CheckItem { get; set; }
        [DisplayName("檢查日期")]
        public string CheckDate { get; set; }
        [DisplayName("檢查時間")]
        public string CheckTime { get; set; }
        [DisplayName("檢查結果")]
        public string Result { get; set; }
        [DisplayName("下限值")]
        public string LowerLimit { get; set; }
        [DisplayName("下限警戒值")]
        public string LowerAlertLimit { get; set; }
        [DisplayName("上限警戒值")]
        public string UpperAlertLimit { get; set; }
        [DisplayName("上限值")]
        public string UpperLimit { get; set; }
        [DisplayName("單位")]
        public string Unit { get; set; }
        [DisplayName("檢查人員")]
        public string User { get; set; }
        [DisplayName("異常原因及處理對策")]
        public string AbnormalReasons { get; set; }
    }
}
