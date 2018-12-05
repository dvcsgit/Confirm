using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.ResultQuery
{
    public class CheckResultExcelItem
    {
        public string Organization { get; set; }

        public string Job { get; set; }

        public string ControlPoint { get; set; }

        public string ArriveTime { get; set; }

        public string User { get; set; }

        public string UnRFIDReason { get; set; }

        public string Equipment { get; set; }

        public string CheckItem { get; set; }

        public string CheckTime { get; set; }

        public string Result { get; set; }

        public string LowerLimit { get; set; }

        public string LowerAlertLimit { get; set; }

        public string UpperAlertLimit { get; set; }

        public string UpperLimit { get; set; }

        public string Unit { get; set; }

        public string AbnormalReason { get; set; }
    }
}
