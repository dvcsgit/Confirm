using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TankPatrol.PortManagement
{
    public class CheckItemModel
    {
        public bool IsInherit { get; set; }

        public string UniqueID { get; set; }

        public string CheckType { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }

        public bool IsFeelItem { get; set; }
        public bool IsAccumulation { get; set; }

        public double? OriUpperLimit { get; set; }
        public double? OriUpperAlertLimit { get; set; }
        public double? OriLowerAlertLimit { get; set; }
        public double? OriLowerLimit { get; set; }
        public double? OriAccumulationBase { get; set; }
        public string OriUnit { get; set; }
        public string OriRemark { get; set; }

        public double? UpperLimit { get; set; }
        public double? UpperAlertLimit { get; set; }
        public double? LowerAlertLimit { get; set; }
        public double? LowerLimit { get; set; }
        public double? AccumulationBase { get; set; }
        public string Unit { get; set; }
        public string Remark { get; set; }

        public string TagID { get; set; }

        public CheckItemModel()
        {
            IsInherit = true;
        }
    }
}
