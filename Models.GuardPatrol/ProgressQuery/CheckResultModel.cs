using System.Text;
using System.Collections.Generic;

namespace Models.GuardPatrol.ProgressQuery
{
    public class CheckResultModel
    {
        public string ArriveRecordUniqueID { get; set; }

        public string UniqueID { get; set; }

        public string CheckDate { get; set; }

        public string CheckTime { get; set; }

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
