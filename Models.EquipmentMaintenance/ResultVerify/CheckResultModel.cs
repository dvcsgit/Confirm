using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.ResultVerify
{
    public class CheckResultModel
    {
        public string ArriveRecordUniqueID { get; set; }

        public string CheckDate { get; set; }

        public string CheckTime { get; set; }

        public bool IsAbnormal { get; set; }

        public bool IsAlert { get; set; }

        public string Result { get; set; }

#if ORACLE
        public decimal? LowerLimit { get; set; }

        public decimal? LowerAlertLimit { get; set; }

        public decimal? UpperAlertLimit { get; set; }

        public decimal? UpperLimit { get; set; }
#else
        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }
#endif

        public string Unit { get; set; }

        public string Remark { get; set; }

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
            AbnormalReasonList = new List<AbnormalReasonModel>();
        }
    }
}
