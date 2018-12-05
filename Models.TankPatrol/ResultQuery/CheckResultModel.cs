using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TankPatrol.ResultQuery
{
    public class CheckResultModel
    {
        public string UniqueID { get; set; }

        public string CheckDate { get; set; }

        public string CheckTime { get; set; }

        public string CheckItemID { get; set; }

        public string CheckItemDescription { get; set; }

        public string CheckItem
        {
            get
            {
                return string.Format("{0}/{1}", CheckItemID, CheckItemDescription);
            }
        }
        public string Result { get; set; }
        public bool IsAbnormal { get; set; }
        public bool IsAlert { get; set; }
        public double? LowerLimit { get; set; }
        public double? LowerAlertLimit { get; set; }
        public double? UpperAlertLimit { get; set; }
        public double? UpperLimit { get; set; }
        public string Unit { get; set; }
        
    }
}
