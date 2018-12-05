using System.Collections.Generic;

namespace Customized.SESC.Models.CheckReport
{
    public class ItemModel
    {
        public int No { get; set; }

        public string ControlPointDescription { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string CheckItemDescription { get; set; }

        public string Description
        {
            get
            {
                if (!string.IsNullOrEmpty(EquipmentName))
                {
                    if (CheckItemDescription.StartsWith(EquipmentName))
                    {
                        return CheckItemDescription;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(PartDescription))
                        {
                            return string.Format("{0}-{1}{2}", EquipmentName, PartDescription, CheckItemDescription);
                        }
                        else
                        {
                            return string.Format("{0}{1}", EquipmentName, CheckItemDescription);
                        }
                    }
                }
                else
                {
                    if (CheckItemDescription.StartsWith(ControlPointDescription))
                    {
                        return CheckItemDescription;
                    }
                    else
                    {
                        return string.Format("{0}{1}", ControlPointDescription, CheckItemDescription);
                    }
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

        public string Unit { get; set; }

        public string Remark { get; set; }

        public Dictionary<string, string> Result { get; set; }

        public ItemModel()
        {
            Result = new Dictionary<string, string>();
        }
    }
}
