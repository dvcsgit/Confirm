using System.Text;
using System.Collections.Generic;

namespace Models.GuardPatrol.DailyReport
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
    }
}
