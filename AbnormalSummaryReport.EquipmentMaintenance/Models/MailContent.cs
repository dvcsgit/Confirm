using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbnormalSummaryReport.EquipmentMaintenance.Models
{
    public class MailContent
    {
        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string RouteUniqueID { get; set; }

        public string RouteID { get; set; }

        public string RouteName { get; set; }

        public string Route
        {
            get
            {
                return string.Format("{0}/{1}", RouteID, RouteName);
            }
        }

        public string Subject
        {
            get
            {
                return string.Format("[{0}]{1}[{2}至{3}]", Resources.Resource.CheckAbnormalNotify, Route, BeginTime.ToString("yyyy-MM-dd HH:mm:ss"), EndTime.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

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
