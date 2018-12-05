using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.CHIMEI.E.Models
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

        public List<OverTimeReasonModel> OverTimeReasonList { get; set; }

        public List<TimeSpanAbnormalReasonModel> TimeSpanAbnormalReasonList { get; set; }

        public List<UnPatrolReasonModel> UnPatrolReasonList { get; set; }

        public List<UnRFIDReasonModel> UnRFIDReasonList { get; set; }

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
            OverTimeReasonList = new List<OverTimeReasonModel>();
            TimeSpanAbnormalReasonList = new List<TimeSpanAbnormalReasonModel>();
            UnRFIDReasonList = new List<UnRFIDReasonModel>();
            UnPatrolReasonList = new List<UnPatrolReasonModel>();
        }
    }
}