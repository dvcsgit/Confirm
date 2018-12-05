using System.Collections.Generic;
using System.Text;

namespace Models.GuardPatrol.DataSync
{
    public class PrevCheckResultModel
    {
        public string ControlPointUniqueID { get; set; }

        public string CheckItemUniqueID { get; set; }

        public string CheckDate { get; set; }

        public string CheckTime { get; set; }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

        public string Unit { get; set; }

        public bool IsAbnormal { get; set; }

        public string Result { get; set; }

        public List<PrevCheckResultAbnormalReasonModel> AbnormalReasonList { get; set; }

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

        public override bool Equals(object Object)
        {
            return Equals(Object as PrevCheckResultModel);
        }

        public override int GetHashCode()
        {
            return ControlPointUniqueID.GetHashCode() + CheckItemUniqueID.GetHashCode();
        }

        public bool Equals(PrevCheckResultModel Model)
        {
            return ControlPointUniqueID.Equals(Model.ControlPointUniqueID) && CheckItemUniqueID.Equals(Model.CheckItemUniqueID);
        }
    }
}
