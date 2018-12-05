using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSI.AbnormalSummaryMail.Models
{
    public class MailContent
    {
        public string OrganizationUniqueID { get; set; }

        public string RouteUniqueID { get; set; }
        
        public List<JobAbnormal> JobAbnormalList { get; set; }

        public List<ControlPointAbnormal> ControlPointAbnormalList { get; set; }

        public List<CheckItemAbnormal> CheckItemAbnormalList { get; set; }

        public bool HaveAbnormal
        {
            get
            {
                return JobAbnormalList.Count + ControlPointAbnormalList.Count + CheckItemAbnormalList.Count > 0;
            }
        }

        public MailContent()
        {
            JobAbnormalList = new List<JobAbnormal>();
            ControlPointAbnormalList = new List<ControlPointAbnormal>();
            CheckItemAbnormalList = new List<CheckItemAbnormal>();
        }
    }
}
