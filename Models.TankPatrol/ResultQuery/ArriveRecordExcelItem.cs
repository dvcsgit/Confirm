using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TankPatrol.ResultQuery
{
    public class ArriveRecordExcelItem
    {
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

        [DisplayName("檢查類別")]
        public string CheckType { get; set; }

        [DisplayName("司機")]
        public string Driver { get; set; }

        [DisplayName("上次承載物質")]
        public string LastTimeMaterial { get; set; }

        [DisplayName("本次承載物質")]
        public string ThisTimeMaterial { get; set; }

        [DisplayName("貨主")]
        public string Owner { get; set; }

        [DisplayName("檢查人員")]
        public string User { get; set; }

        [DisplayName("到位日期")]
        public string ArriveDate { get; set; }

        [DisplayName("到位時間")]
        public string ArriveTime { get; set; }

        [DisplayName("司機簽名")]
        public string Sign { get; set; }
    }
}
