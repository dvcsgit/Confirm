using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.SESC.Models.DCSReport
{
    public class ItemModel
    {
        public int No { get; set; }

        public string EquipmentName { get; set; }

        public string CheckItemDescription { get; set; }

        public string CheckItemDisplay
        {
            get
            {
                return CheckItemDescription.Replace("【DCS】", "").ToString();
            }
        }

        public string Unit { get; set; }

        public string DCSCheckItemID { get; set; }

        public double? DCSValue { get; set; }

        public string FactoryCheckItemID { get; set; }

        public double? FactoryValue { get; set; }

        public string Diff
        {
            get
            {
                if (DCSValue.HasValue && FactoryValue.HasValue)
                {
                    return (DCSValue.Value - FactoryValue.Value).ToString();
                }
                else
                {
                    return "-";
                }
            }
        }

        public double? LowerLimit { get; set; }

        public double? UpperLimit { get; set; }

        public string Limit
        {
            get
            {
                if (LowerLimit.HasValue && UpperLimit.HasValue)
                {
                    return string.Format("{0}~{1}", LowerLimit.Value, UpperLimit.Value);
                }
                else if (!LowerLimit.HasValue && UpperLimit.HasValue)
                {
                    return string.Format("<{0}", UpperLimit.Value);
                }
                else if (LowerLimit.HasValue && !UpperLimit.HasValue)
                {
                    return string.Format(">{0}", LowerLimit.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
