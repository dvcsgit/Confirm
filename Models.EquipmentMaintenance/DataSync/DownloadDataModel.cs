using System.Linq;
using System.Collections.Generic;

namespace Models.EquipmentMaintenance.DataSync
{
    public class DownloadDataModel
    {
        public List<MaintenanceFormModel> MaintenanceFormList { get; set; }

        public List<RepairFormModel> RepairFormList { get; set; }

        public List<JobModel> JobList { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return JobList.SelectMany(x => x.CheckItemList).Distinct().ToList();
            }
        }

        public List<StandardModel> StandardList
        {
            get
            {
                return MaintenanceFormList.SelectMany(x => x.StandardList).Distinct().ToList();
            }
        }

        public List<AbnormalReasonModel> AbnormalReasonList
        {
            get
            {
                return CheckItemList.SelectMany(x => x.AbnormalReasonList).Union(MaintenanceFormList.SelectMany(x => x.AbnormalReasonList)).Distinct().ToList();
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

        public List<EmgContactModel> EmgContactList
        {
            get
            {
                return JobList.SelectMany(x => x.EmgContactList).Distinct().ToList();
            }
        }

        public List<UserModel> AllUserList { get; set; }

        public List<UserModel> UserList
        {
            get
            {
                return JobList.SelectMany(x => x.UserList).Union(RepairFormList.SelectMany(x => x.UserList).ToList()).Union(MaintenanceFormList.SelectMany(x => x.UserList).ToList()).Distinct().ToList();
            }
        }

        public Dictionary<string, string> LastModifyTimeList
        {
            get
            {
                return JobList.ToDictionary(x => x.UniqueID, x => x.LastModifyTime);
            }
        }

        public List<EquipmentSpecModel> EquipmentSpecList
        {
            get
            {
                return JobList.SelectMany(x => x.EquipmentSpecList).Distinct().ToList();
            }
        }

        public List<MaterialModel> MaterialList
        {
            get
            {
                return JobList.SelectMany(x => x.MaterialList).Distinct().ToList();
            }
        }

        public List<MaterialSpecModel> MaterialSpecList
        {
            get
            {
                return MaterialList.SelectMany(x => x.SpecList).Distinct().ToList();
            }
        }

        public DownloadDataModel()
        {
            MaintenanceFormList = new List<MaintenanceFormModel>();
            RepairFormList = new List<RepairFormModel>();
            JobList = new List<JobModel>();
            OverTimeReasonList = new List<OverTimeReason>();
            TimeSpanAbnormalReasonList = new List<TimeSpanAbnormalReason>();
            UnRFIDReasonList = new List<UnRFIDReason>();
            UnPatrolReasonList = new List<UnPatrolReason>();
            PrevCheckResultList = new List<PrevCheckResultModel>();
            AllUserList = new List<UserModel>();
        }
    }
}
