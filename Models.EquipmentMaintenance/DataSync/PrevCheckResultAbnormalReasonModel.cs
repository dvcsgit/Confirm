using System.Collections.Generic;
using System.Text;
namespace Models.EquipmentMaintenance.DataSync
{
    public class PrevCheckResultAbnormalReasonModel
    {
        public string Description { get; set; }

        public string Remark { get; set; }

        public string AbnormalReason
        {
            get
            {
                if (!string.IsNullOrEmpty(Remark))
                {
                    return string.Format("{0}({1})", Description, Remark);
                }
                else
                {
                    return Description;
                }
            }
        }

        public List<PrevCheckResultHandlingMethodModel> HandlingMethodList { get; set; }

        public string HandlingMethods
        {
            get
            {
                if (HandlingMethodList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var handlingMethod in HandlingMethodList)
                    {
                        sb.Append(handlingMethod.HandlingMethod);

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

        public PrevCheckResultAbnormalReasonModel()
        {
            HandlingMethodList = new List<PrevCheckResultHandlingMethodModel>();
        }
    }
}
