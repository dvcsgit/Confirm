using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SQLite2DB.EquipmentMaintenance.Models
{
    public class JobResultModel
    {
        public string UniqueID { get; set; }

        public string JobUniqueID { get; set; }

        public string BeginDate { get; set; }

        public string EndDate { get; set; }

        public string CurrentOverTimeReason { get; set; }

        public string NewOverTimeReasonDescription { get; set; }

        public string NewOverTimeReasonRemark { get; set; }

        public string NewOverTimeReason
        {
            get
            {
                if (!string.IsNullOrEmpty(NewOverTimeReasonDescription))
                {
                    return NewOverTimeReasonDescription;
                }
                else
                {
                    return NewOverTimeReasonRemark;
                }
            }
        }

        public string OverTimeReason
        {
            get
            {
                if (!string.IsNullOrEmpty(NewOverTimeReason))
                {
                    return NewOverTimeReason;
                }
                else
                {
                    return CurrentOverTimeReason;
                }
            }
        }

        public string CurrentOverTimeUser { get; set; }

        public UserModel NewOverTimeUser { get; set; }

        public string OverTimeUser
        {
            get
            {
                if (NewOverTimeUser!=null)
                {
                    return NewOverTimeUser.User;
                }
                else
                {
                    return CurrentOverTimeUser;
                }
            }
        }

        public string CurrentUnPatrolReason { get; set; }

        public string NewUnPatrolReasonDescription { get; set; }

        public string NewUnPatrolReasonRemark { get; set; }

        public string NewUnPatrolReason
        {
            get
            {
                if (!string.IsNullOrEmpty(NewUnPatrolReasonDescription))
                {
                    return NewUnPatrolReasonDescription;
                }
                else
                {
                    return NewUnPatrolReasonRemark;
                }
            }
        }

        public string UnPatrolReason
        {
            get
            {
                if (!string.IsNullOrEmpty(NewUnPatrolReason))
                {
                    return NewUnPatrolReason;
                }
                else
                {
                    return CurrentUnPatrolReason;
                }
            }
        }

        public string CurrentUnPatrolUser { get; set; }

        public UserModel NewUnPatrolUser { get; set; }

        public string UnPatrolUser
        {
            get
            {
                if (NewUnPatrolUser != null)
                {
                    return NewUnPatrolUser.User;
                }
                else
                {
                    return CurrentUnPatrolUser;
                }
            }
        }
    }
}
