using System;
using Utility;

namespace Models.EquipmentMaintenance.QFormManagement
{
    public class GridItem
    {
        public int Status
        {
            get
            {
                if (IsClosed)
                {
                    return 3;
                }
                else
                {
                    if (TakeJobTime.HasValue)
                    {
                        return 2;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
        }

        public string StatusDescription
        {
            get
            {
                switch (Status)
                { 
                    case 1:
                        return Resources.Resource.QFormStatus_1;
                    case 2:
                        return Resources.Resource.QFormStatus_2;
                    case 3:
                        return Resources.Resource.QFormStatus_3;
                    default:
                        return "-";
                }
            }
        }

        public string UniqueID { get; set; }

        public string VHNO { get; set; }

        public string Subject { get; set; }

        public string ContactName { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public string JobUserID { get; set; }

        public string JobUserName { get; set; }

        public string JobUser
        {
            get
            {
                if (!string.IsNullOrEmpty(JobUserName))
                {
                    return string.Format("{0}/{1}", JobUserID, JobUserName);
                }
                else
                {
                    return JobUserID;
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

        public DateTime? ClosedTime { get; set; }

        public string ClosedTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(ClosedTime);
            }
        }

        public bool IsClosed { get; set; }
    }
}
