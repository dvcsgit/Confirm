using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TruckPatrol.ResultQuery
{
    public class TruckBindingResultExcelItem
    {
        [DisplayName("異常")]
        public string Abnormal { get; set; }
        [DisplayName("車頭")]
        public string CarNo { get; set; }
        [DisplayName("尾車")]
        public string SecondCarNo { get; set; }
        [DisplayName("檢查日期")]
        public string CheckDate { get; set; }
        [DisplayName("檢查人員")]
        public string CheckUser { get; set; }
        [DisplayName("完成率")]
        public string ComplateRate { get; set; }
        [DisplayName("作業時間")]
        public string TimeSpan { get; set; }
    }
}
