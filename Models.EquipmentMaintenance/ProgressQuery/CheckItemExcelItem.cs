using System.Collections.Generic;
using System.ComponentModel;

namespace Models.EquipmentMaintenance.ProgressQuery
{
    public class CheckItemExcelItem
    {
        public string JobDescription { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public string ControlPoint { get; set; }

        public string IsAbnormal { get; set; }

        public string Equipment { get; set; }

        public string CheckItem { get; set; }

        public string CheckDate { get; set; }

        public string CheckTime { get; set; }

        public string Result { get; set; }

        public string LowerLimit { get; set; }

        public string LowerAlertLimit { get; set; }

        public string UpperAlertLimit { get; set; }

        public string UpperLimit { get; set; }

        public string Unit { get; set; }

        public string User { get; set; }

        public string AbnormalReasons { get; set; }

        public List<string> PhotoList { get; set; }

        public CheckItemExcelItem()
        {
            PhotoList = new List<string>();
        }
    }
}
