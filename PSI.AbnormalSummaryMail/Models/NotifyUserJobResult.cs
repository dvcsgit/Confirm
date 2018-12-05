using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace PSI.AbnormalSummaryMail.Models
{
    public class NotifyUserJobResult
    {
        public string UserID { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime JobEndTime { get; set; }

        public string Subject
        {
            get
            {
                return string.Format("【巡檢派工即將逾期通知】共有「{0}」筆尚未完成之派工作業即將於「{1}」逾期", JobResultList.Count, DateTimeHelper.DateTime2DateTimeStringWithSeperator(JobEndTime));
            }
        }

        public List<JobResult> JobResultList { get; set; }

        public NotifyUserJobResult()
        {
            JobResultList = new List<JobResult>();
        }
    }
}
