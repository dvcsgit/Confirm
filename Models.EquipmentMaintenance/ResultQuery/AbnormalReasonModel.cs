using System.Text;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.ResultQuery
{
    public class AbnormalReasonModel
    {
        public string Description { get; set; }

        public string Remark { get; set; }

        public string AbnormalReason
        {
            get
            {
                if (string.IsNullOrEmpty(Remark))
                {
                    return Description;
                }
                else
                {
                    return string.Format("{0}({1})", Description, Remark);
                }
            }
        }

        public List<HandlingMethodModel> HandlingMethodList { get; set; }

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

        public AbnormalReasonModel()
        {
            HandlingMethodList = new List<HandlingMethodModel>();
        }
    }
}
