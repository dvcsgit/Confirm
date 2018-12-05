using System.Linq;
using System.Collections.Generic;
using DbEntity.MSSQL.GuardPatrol;

namespace Models.GuardPatrol.DataSync
{
    public class DownloadDataModel
    {
        public List<JobModel> JobList { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return JobList.SelectMany(x => x.CheckItemList).Distinct().ToList();
            }
        }

        public List<AbnormalReasonModel> AbnormalReasonList
        {
            get
            {
                return CheckItemList.SelectMany(x => x.AbnormalReasonList).Distinct().ToList();
            }
        }

        public List<HandlingMethodModel> HandlingMethodList
        {
            get
            {
                return AbnormalReasonList.SelectMany(x => x.HandlingMethodList).Distinct().ToList();
            }
        }

        public List<PrevCheckResultModel> PrevCheckResultList { get; set; }

        public List<OverTimeReason> OverTimeReasonList { get; set; }

        public List<TimeSpanAbnormalReason> TimeSpanAbnormalReasonList { get; set; }

        public List<UnPatrolReason> UnPatrolReasonList { get; set; }

        public List<UnRFIDReason> UnRFIDReasonList { get; set; }

        public List<UserModel> UserList
        {
            get
            {
                return JobList.SelectMany(x => x.UserList).Distinct().ToList();
            }
        }

        public Dictionary<string, string> LastModifyTimeList
        {
            get
            {
                return JobList.ToDictionary(x => x.UniqueID, x => x.LastModifyTime);
            }
        }

        public DownloadDataModel()
        {
            JobList = new List<JobModel>();
            OverTimeReasonList = new List<OverTimeReason>();
            TimeSpanAbnormalReasonList = new List<TimeSpanAbnormalReason>();
            UnRFIDReasonList = new List<UnRFIDReason>();
            PrevCheckResultList = new List<PrevCheckResultModel>();
        }
    }
}
