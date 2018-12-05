using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Home
{
    public class EquipmentPatrolVerifyItem
    {
        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string Description { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public bool HaveAbnormal { get; set; }

        public bool HaveAlert { get; set; }

        public string CompleteRate { get; set; }

        public string CompleteRateLabelClass { get; set; }

        public string TimeSpan { get; set; }

        public string CheckUsers { get; set; }

        public string ArriveStatus { get; set; }

        public string ArriveStatusLabelClass { get; set; }

        public bool IsClosed { get; set; }

        public string CurrentVerifyUserID { get; set; }

        public string CurrentVerifyUserName { get; set; }

        public string CurrentVerifyUser
        {
            get
            {
                if (!string.IsNullOrEmpty(CurrentVerifyUserName))
                {
                    return string.Format("{0}/{1}", CurrentVerifyUserID, CurrentVerifyUserName);
                }
                else
                {
                    return CurrentVerifyUserID;
                }
            }
        }
    }
}
