using Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class FormViewModel
    {
        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public string Status { get; set; }

        public string StatusCode
        {
            get
            {
                if (Status == "0")
                {
                    if (DateTime.Compare(DateTime.Today, EstEndDate) > 0)
                    {
                        return "7";
                    }
                    else
                    {
                        return "0";
                    }
                }
                else if (Status == "1")
                {
                    if (DateTime.Compare(DateTime.Today, EstEndDate) > 0)
                    {
                        return "2";
                    }
                    else
                    {
                        return "1";
                    }
                }
                else
                {
                    return Status;
                }
            }
        }

        [Display(Name = "Status", ResourceType = typeof(Resources.Resource))]
        public string StatusDescription
        {
            get
            {
                switch (StatusCode)
                {
                    case "0":
                        return Resources.Resource.MFormStatus_0;
                    case "1":
                        return Resources.Resource.MFormStatus_1;
                    case "2":
                        return Resources.Resource.MFormStatus_2;
                    case "3":
                        return Resources.Resource.MFormStatus_3;
                    case "4":
                        return Resources.Resource.MFormStatus_4;
                    case "5":
                        return Resources.Resource.MFormStatus_5;
                    case "6":
                        return Resources.Resource.MFormStatus_6;
                    case "7":
                        return "未接案(逾期)";
                    default:
                        return "-";
                }
            }
        }

        [Display(Name = "Subject", ResourceType = typeof(Resources.Resource))]
        public string Subject { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        [Display(Name = "Equipment", ResourceType = typeof(Resources.Resource))]
        public string Equipment
        {
            get
            {
                if (!string.IsNullOrEmpty(PartDescription))
                {
                    return string.Format("{0}/{1}-{2}", EquipmentID, EquipmentName, PartDescription);
                }
                else
                {
                    return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                }
            }
        }

        public DateTime CycleBeginDate { get; set; }

        [Display(Name = "CycleBeginDate", ResourceType = typeof(Resources.Resource))]
        public string CycleBeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(CycleBeginDate);
            }
        }

        public DateTime CycleEndDate { get; set; }

        [Display(Name = "CycleEndDate", ResourceType = typeof(Resources.Resource))]
        public string CycleEndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(CycleEndDate);
            }
        }


        public DateTime EstBeginDate { get; set; }

        [Display(Name = "EstMaintenanceBeginDate", ResourceType = typeof(Resources.Resource))]
        public string EstBeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstBeginDate);
            }
        }

        public DateTime EstEndDate { get; set; }

        [Display(Name = "EstMaintenanceEndDate", ResourceType = typeof(Resources.Resource))]
        public string EstEndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstEndDate);
            }
        }

        public DateTime? BeginDate { get; set; }

        [Display(Name = "MaintenanceBeginDate", ResourceType = typeof(Resources.Resource))]
        public string BeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(BeginDate);
            }
        }

        public DateTime? EndDate { get; set; }

        [Display(Name = "MaintenanceEndDate", ResourceType = typeof(Resources.Resource))]
        public string EndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EndDate);
            }
        }

        public List<UserModel> JobUserList { get; set; }

        [Display(Name = "JobUser", ResourceType = typeof(Resources.Resource))]
        public string JobUser
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (JobUserList.Count > 0)
                {
                    foreach (var user in JobUserList)
                    {
                        sb.Append(user.User);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }

        public DateTime CreateTime { get; set; }

        [Display(Name = "CreateTime", ResourceType = typeof(Resources.Resource))]
        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public string TakeJobUserID { get; set; }

        public string TakeJobUserName { get; set; }

        [Display(Name = "TakeJobUser", ResourceType = typeof(Resources.Resource))]
        public string TakeJobUser
        {
            get
            {
                if (!string.IsNullOrEmpty(TakeJobUserName))
                {
                    return string.Format("{0}/{1}", TakeJobUserID, TakeJobUserName);
                }
                else
                {
                    return TakeJobUserID;
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

        public List<UserModel> MaintenanceUserList { get; set; }

        [Display(Name = "MaintenanceUser", ResourceType = typeof(Resources.Resource))]
        public string MaintenanceUser
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (MaintenanceUserList.Count > 0)
                {
                    foreach (var user in MaintenanceUserList)
                    {
                        sb.Append(user.User);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }

        public List<StandardModel> StandardList { get; set; }

        public List<ResultModel> ResultList
        {
            get
            {
                return StandardList.SelectMany(x => x.ResultList).ToList();
            }
        }

        //public List<StandardResultModel> StandardResultList
        //{
        //    get
        //    {
        //        return StandardList.SelectMany(x => x.ResultList).ToList();
        //    }
        //}

        public List<MaterialModel> MaterialList { get; set; }

        public List<FlowLogModel> FlowLogList { get; set; }

        public List<ExtendLogModel> ExtendLogList { get; set; }

        public List<FileModel> FileList { get; set; }

        public List<WorkingHourModel> WorkingHourList { get; set; }

        public string CurrentVerifyUserID
        {
            get
            {
                if (FlowLogList != null && FlowLogList.Count > 0)
                {
                    var log = FlowLogList.FirstOrDefault(x => !x.VerifyTime.HasValue);

                    if (log != null)
                    {
                        return log.User.ID;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string CurrentExtendVerifyUserID
        {
            get
            {
                if (ExtendLogList != null && ExtendLogList.Count > 0)
                {
                    var extendLog = ExtendLogList.FirstOrDefault(x => !x.IsClosed);

                    if (extendLog != null)
                    {
                        var log = extendLog.LogList.FirstOrDefault(x => !x.VerifyTime.HasValue);

                        if (log != null)
                        {
                            return log.UserID;
                        }
                        else
                        {
                            return string.Empty;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public FormViewModel()
        {
            JobUserList = new List<UserModel>();
            MaintenanceUserList = new List<UserModel>();
            StandardList = new List<StandardModel>();
            MaterialList = new List<MaterialModel>();
            ExtendLogList = new List<ExtendLogModel>();
            FlowLogList = new List<FlowLogModel>();
            FileList = new List<FileModel>();
            WorkingHourList = new List<WorkingHourModel>();
            //ResultList = new List<ResultModel>();
        }
    }
}
