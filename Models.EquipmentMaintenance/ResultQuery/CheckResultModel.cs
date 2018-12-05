using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.ResultQuery
{
    public class CheckResultModel
    {
        public string UniqueID { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string Equipment
        {
            get
            {
                if (!string.IsNullOrEmpty(EquipmentID))
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
                else
                {
                    return string.Empty;
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

        public string Date { get; set; }

        public string Time { get; set; }

        public string CheckTime
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(Date, Time).Value);
            }
        }

        public bool IsAbnormal { get; set; }

        public bool IsAlert { get; set; }

        public string Result { get; set; }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

        public string Unit { get; set; }

        public List<string> PhotoList { get; set; }

        public bool HavePhoto
        {
            get
            {
                return PhotoList.Count > 0;
            }
        }

        public List<AbnormalReasonModel> AbnormalReasonList { get; set; }

        public string AbnormalReasons
        {
            get
            {
                if (AbnormalReasonList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var abnormalReason in AbnormalReasonList)
                    {
                        if (!string.IsNullOrEmpty(abnormalReason.HandlingMethods))
                        {
                            sb.Append(string.Format("{0}({1})", abnormalReason.AbnormalReason, abnormalReason.HandlingMethods));
                        }
                        else
                        {
                            sb.Append(abnormalReason.AbnormalReason);
                        }

                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public CheckResultModel()
        {
            PhotoList = new List<string>();
            AbnormalReasonList = new List<AbnormalReasonModel>();
        }
    }
}
