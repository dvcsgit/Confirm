using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.CHIMEI.Models.AIMSJobQuery
{
    public class ExcelItem
    {
        [DisplayName("異常")]
        public string Abnormal { get; set; }

        [DisplayName("設備代號")]
        public string EquipmentID { get; set; }

        [DisplayName("設備名稱")]
        public string EquipmentName { get; set; }

        [DisplayName("檢查項目代號")]
        public string CheckItemID { get; set; }

        [DisplayName("檢查項目描述")]
        public string CheckItemDescription { get; set; }

        [DisplayName("檢查日期")]
        public string CheckDate { get; set; }

        [DisplayName("檢查時間")]
        public string CheckTime { get; set; }

        [DisplayName("檢查結果")]
        public string Result { get; set; }

        [DisplayName("上限警戒值")]
        public string UpperAlertLimit { get; set; }

        [DisplayName("上限值")]
        public string UpperLimit { get; set; }

        [DisplayName("單位")]
        public string Unit { get; set; }

        [DisplayName("異常原因及處理對策")]
        public string AbnormalReasons { get; set; }
    }
}
