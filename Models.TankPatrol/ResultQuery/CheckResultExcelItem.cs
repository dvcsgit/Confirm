using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TankPatrol.ResultQuery
{
    public class CheckResultExcelItem
    {
        [DisplayName("異常")]
        public string IsAbnormal { get; set; }

        [DisplayName("組織")]
        public string Organization { get; set; }

        [DisplayName("裝卸料站")]
        public string Station { get; set; }

        [DisplayName("灌島")]
        public string Island { get; set; }

        [DisplayName("灌口")]
        public string Port { get; set; }

        [DisplayName("車牌號碼")]
        public string TankNo { get; set; }

        [DisplayName("檢查人員")]
        public string User { get; set; }

        [DisplayName("檢查日期")]
        public string CheckDate { get; set; }

        [DisplayName("檢查時間")]
        public string CheckTime { get; set; }

        [DisplayName("檢查類別")]
        public string CheckType { get; set; }

        [DisplayName("檢查項目")]
        public string CheckItem { get; set; }

        [DisplayName("檢查結果")]
        public string Result { get; set; }

        [DisplayName("下限值")]
        public double? LowerLimit { get; set; }

        [DisplayName("下限警戒值")]
        public double? LowerAlertLimit { get; set; }

        [DisplayName("上限警戒值")]
        public double? UpperAlertLimit { get; set; }

        [DisplayName("上限值")]
        public double? UpperLimit { get; set; }

        [DisplayName("單位")]
        public string Unit { get; set; }
    }
}
