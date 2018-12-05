using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.QFormManagement
{
    public class DetailViewModel
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

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        [Display(Name = "Subject", ResourceType = typeof(Resources.Resource))]
        public string Subject { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [Display(Name = "Contact", ResourceType = typeof(Resources.Resource))]
        public string ContactName { get; set; }

        [Display(Name = "ContactTel", ResourceType = typeof(Resources.Resource))]
        public string ContactTel { get; set; }

        [Display(Name = "ContactEmail", ResourceType = typeof(Resources.Resource))]
        public string ContactEmail { get; set; }

        public DateTime CreateTime { get; set; }

        [Display(Name = "CreateTime", ResourceType = typeof(Resources.Resource))]
        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public string JobUserID { get; set; }

        public string JobUserName { get; set; }

        [Display(Name = "TakeJobUser", ResourceType = typeof(Resources.Resource))]
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

        [Display(Name = "TakeJobTime", ResourceType = typeof(Resources.Resource))]
        public string TakeJobTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(TakeJobTime);
            }
        }

        [Display(Name = "Comment", ResourceType = typeof(Resources.Resource))]
        public string Comment { get; set; }

        public bool IsClosed { get; set; }

        public DateTime? ClosedTime { get; set; }

        [Display(Name = "ClosedTime", ResourceType = typeof(Resources.Resource))]
        public string ClosedTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(ClosedTime);
            }
        }

        public List<RepairFormModel> RepairFormList { get; set; }

        public DetailViewModel()
        {
            RepairFormList = new List<RepairFormModel>();
        }
    }
}
