using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.TrendQuery_CheckItem
{
    public class GridItem
    {
        public Define.EnumTreeNodeType Type { get; set; }

        public string ControlPointUniqueID { get; set; }

        public string EquipmentUniqueID { get; set; }

        public string PartUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string ControlPointID { get; set; }

        public string ControlPointDescription { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string ID
        {
            get
            {
                if (Type == Define.EnumTreeNodeType.ControlPoint)
                {
                    return ControlPointID;
                }
                else if (Type == Define.EnumTreeNodeType.Equipment)
                {
                    return EquipmentID;
                }
                else if (Type == Define.EnumTreeNodeType.EquipmentPart)
                {
                    return EquipmentID;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Name
        {
            get
            {
                if (Type == Define.EnumTreeNodeType.ControlPoint)
                {
                    return ControlPointDescription;
                }
                else if (Type == Define.EnumTreeNodeType.Equipment)
                {
                    return EquipmentName;
                }
                else if (Type == Define.EnumTreeNodeType.EquipmentPart)
                {
                    return string.Format("{0}-{1}", EquipmentName, PartDescription);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }
    }
}
