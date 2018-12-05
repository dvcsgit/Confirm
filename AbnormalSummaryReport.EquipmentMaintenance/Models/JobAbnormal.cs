using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace AbnormalSummaryReport.EquipmentMaintenance.Models
{
    public class JobAbnormal
    {
        public string JobDescription { get; set; }

        public string BeginDate { get; set; }

        public string BeginTime { get; set; }

        public string Begin
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(BeginDate, BeginTime));
            }
        }

        public string EndDate { get; set; }

        public string EndTime { get; set; }

        public string End
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(EndDate, EndTime));
            }
        }

        public string CompleteRate { get; set; }

        public string UnPatrolReason { get; set; }

        public string ArriveStatus { get; set; }

        public string OverTimeReason { get; set; }
    }
}
