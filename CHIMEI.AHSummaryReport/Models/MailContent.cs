using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace CHIMEI.AHSummaryReport.Models
{
    public class MailContent
    {
        public string OrganizationDescription { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string EquipmentDisplay
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

        public string CheckDate { get; set; }

        public string CheckDateString
        {
            get
            {
                return DateTimeHelper.DateString2DateStringWithSeparator(CheckDate);
            }
        }

        public string Abnormal
        {
            get
            { 
                if(IsAbnormal)
                {
                    return "異常";
                }
                else if (IsAlert)
                {
                    return "注意";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool IsAbnormal { get; set; }

        public bool IsAlert { get; set; }

        public List<string> CheckUserList { get; set; }

        public string CheckUsers
        {
            get
            {
                if (CheckUserList != null && CheckUserList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var checkUser in CheckUserList)
                    {
                        sb.Append(checkUser);
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

        public MailContent()
        {
            CheckUserList = new List<string>();
        }
    }
}
