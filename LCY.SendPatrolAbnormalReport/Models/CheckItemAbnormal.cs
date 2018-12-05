using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace LCY.SendPatrolAbnormalReport.Models
{
    public class CheckItemAbnormal
    {
        public string JobDescription { get; set; }

        public string ControlPointID { get; set; }

        public string ControlPointDescription { get; set; }

        public string ControlPoint
        {
            get
            {
                return string.Format("{0}/{1}", ControlPointID, ControlPointDescription);
            }
        }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string Equipment
        {
            get
            {
                if (!string.IsNullOrEmpty(PartDescription))
                {
                    return string.Format("{0}/{1}-{2}", EquipmentID, EquipmentName, PartDescription);
                }
                else
                {
                    return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                }
            }
        }

        public string CheckItemID { get; set; }

        public string CheckItemDescription { get; set; }

        public string CheckItem
        {
            get
            {
                return string.Format("{0}/{1}", CheckItemID, CheckItemDescription);
            }
        }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

        public string Unit { get; set; }

        public string Result { get; set; }

        public string CheckDate { get; set; }

        public string CheckTime { get; set; }

        public string CheckDateTime
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(CheckDate, CheckTime));
            }
        }

        public bool IsAbnormal { get; set; }

        public bool IsAlert { get; set; }

        public string AbnormalStatus
        {
            get
            {
                if (IsAbnormal)
                {
                    return Resources.Resource.Abnormal;
                }
                else
                {
                    if (IsAlert)
                    {
                        return Resources.Resource.Warning;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }
    }
}
