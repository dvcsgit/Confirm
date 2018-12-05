using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.CHIMEI.Models.AbnormalHandlingManagement
{
    public class CheckResultModel
    {
        public string AbnormalUniqueID { get; set; }

        public string CheckItemID { get; set; }

        public string CheckItemDescription { get; set; }

        public string CheckItemDisplay
        {
            get
            {
                return string.Format("{0}/{1}", CheckItemID, CheckItemDescription);
            }
        }

        public string CheckTime { get; set; }

        public string CheckTimeString
        {
            get
            {
                return DateTimeHelper.TimeString2TimeStringWithSeperator(CheckTime);
            }
        }

        public bool IsAbnormal { get; set; }

        public bool IsAlert { get; set; }

        public double? LowerLimit { get; set; }
        public double? LowerAlertLimit { get; set; }
        public double? UpperAlertLimit { get; set; }
        public double? UpperLimit { get; set; }

        public string Unit { get; set; }

        public string Result { get; set; }

        public UserModel CheckUser { get; set; }

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
                            sb.Append(string.Format("{0}({1})", abnormalReason.Display, abnormalReason.HandlingMethods));
                        }
                        else
                        {
                            sb.Append(abnormalReason.Display);
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
            CheckUser = new UserModel();
            AbnormalReasonList = new List<AbnormalReasonModel>();
        }
    }
}
