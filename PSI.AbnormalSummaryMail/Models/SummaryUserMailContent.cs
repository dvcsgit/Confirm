using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace PSI.AbnormalSummaryMail.Models
{
   public class SummaryUserMailContent
    {
        public string UserID { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public DateTime EndTime { get; set; }

        public string Subject
        {
            get
            {
                return string.Format("【巡檢結果異常通知】派工結束時間「{0}」", DateTimeHelper.DateTime2DateTimeStringWithSeperator(EndTime));
            }
        }

        public List<JobAbnormal> JobAbnormalList { get; set; }

        public List<ControlPointAbnormal> ControlPointAbnormalList { get; set; }

        public List<CheckItemAbnormal> CheckItemAbnormalList { get; set; }

        public SummaryUserMailContent()
        {
            JobAbnormalList = new List<JobAbnormal>();
            ControlPointAbnormalList = new List<ControlPointAbnormal>();
            CheckItemAbnormalList = new List<CheckItemAbnormal>();
        }
    }
}
