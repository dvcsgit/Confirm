using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.AbnormalNotify.AbnormalNotify
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string VHNO { get; set; }

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
    }
}
