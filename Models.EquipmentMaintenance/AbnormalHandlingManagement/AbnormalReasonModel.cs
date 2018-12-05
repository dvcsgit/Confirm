using System.Text;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.AbnormalHandlingManagement
{
    public class AbnormalReasonModel
    {
        public string Description { get; set; }

        public string Remark { get; set; }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(Remark))
                {
                    return Remark;
                }
                else
                {
                    return Description;
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
                        sb.Append(handlingMethod.Display);

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
