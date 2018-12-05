using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.AbnormalNotify_v2
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public string Status { get; set; }

        public string StatusDescription
        {
            get
            {
                if (Status == "1")
                {
                    return "未接案";
                }
                else if (Status == "2")
                {
                    return "已接案未處置";
                }
                else if (Status == "3")
                {
                    return "呈核中(已即時處理)";
                }
                else if (Status == "4")
                {
                    return "呈核中(提報預計完成日)";
                }
                else if (Status == "5")
                {
                    return "待處置";
                }
                else if (Status == "6")
                {
                    return "呈核中(處理完成)";
                }
                else if (Status == "7")
                {
                    return "已結案";
                }
                else
                {
                    return "-";
                }
            }
        }

        public string Subject { get; set; }

        public DateTime? OccurTime { get; set; }

        public string OccurTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(OccurTime);
            }
        }

        public DateTime CreateTime { get; set; }

        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public string CreateUserID { get; set; }

        public string CreateUserName { get; set; }

        public string CreateUser
        {
            get
            {
                if (!string.IsNullOrEmpty(CreateUserName))
                {
                    return string.Format("{0}/{1}", CreateUserID, CreateUserName);
                }
                else
                {
                    return CreateUserID;
                }
            }
        }

        public string TakeJobUserID { get; set; }

        public string TakeJobUserName { get; set; }

        public string TakeJobUser
        {
            get
            {
                if (!string.IsNullOrEmpty(TakeJobUserName))
                {
                    return string.Format("{0}/{1}", TakeJobUserID, TakeJobUserName);
                }
                else
                {
                    return TakeJobUserID;
                }
            }
        }

        public DateTime? TakeJobTime { get; set; }

        public string TakeJobTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(TakeJobTime);
            }
        }

        public string ResponsibleOrganization { get; set; }

        public List<string> ResponsibleOrganizationList { get; set; }

        public DateTime? ClosedTime { get; set; }

        public string ClosedTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(ClosedTime);
            }
        }

        public GridItem()
        {
            ResponsibleOrganizationList = new List<string>();
        }
    }
}
