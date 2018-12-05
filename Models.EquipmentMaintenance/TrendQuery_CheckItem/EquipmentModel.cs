using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.TrendQuery_CheckItem
{
    public class EquipmentModel
    {
        public string ControlPointDescription { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string Equipment
        {
            get
            {
                if (!string.IsNullOrEmpty(PartDescription))
                {
                    return string.Format("{0}-{1}", EquipmentName, PartDescription);
                }
                else
                {
                    return EquipmentName;
                }
            }
        }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(ControlPointDescription))
                {
                    return ControlPointDescription;
                }
                else
                {
                    return Equipment;
                }
            }
        }

        public string Color { get; set; }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

        public List<CheckResultModel> CheckResultList { get; set; }

        public EquipmentModel()
        {
            CheckResultList = new List<CheckResultModel>();
        }
    }
}


