using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class StandardResultModel
    {
        public string UniqueID { get; set; }

        public string StandardUniqueID { get; set; }

        public string StandardID { get; set; }

        public string StandardDescription { get; set; }

        public bool IsAlert { get; set; }

        public bool IsAbnormal { get; set; }
        
        public string Result {get;set; }

        public string FeelOptionUniqueID { get; set; }

        public string FeelOptionDescription { get; set; }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

        public string Unit { get; set; }

        public double? NetValue { get; set; }

        public double? Value { get; set; }
    }
}
