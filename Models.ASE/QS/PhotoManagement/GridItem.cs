using System;
using Utility;
namespace Models.ASE.QS.PhotoManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string Extension { get; set; }

        public string FileName
        {
            get
            {
                return string.Format("{0}.{1}", UniqueID, Extension);
            }
        }

        public string UserID { get; set; }

        public string UserName { get; set; }

        public string User
        {
            get
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    return string.Format("{0}/{1}", UserID, UserName);
                }
                else
                {
                    return UserID;
                }
            }
        }

        public DateTime? PhotoTime { get; set; }

        public string PhotoTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(PhotoTime);
            }
        }
    }
}
