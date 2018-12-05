using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;
using System.Linq;
using Models.Shared;
using System.Web.Mvc;
using System.ComponentModel;
using System.Text;

namespace Models.EquipmentMaintenance.RepairFormManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public string AncestorOrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public string OrganizationDescription { get; set; }

        [Display(Name = "MaintenanceOrganization", ResourceType = typeof(Resources.Resource))]
        public string MaintenanceOrganizationFullDescription { get; set; }

        public string MaintenanceOrganizationDescription { get; set; }
        

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        public string Status { get; set; }

        public string StatusCode
        {
            get
            {
                if (Status == "4")
                {
                    if (DateTime.Compare(DateTime.Today, EstEndDate.Value) >= 0)
                    {
                        return "5";
                    }
                    else
                    {
                        return Status;
                    }
                }
                else
                {
                    return Status;
                }
            }
        }

        public string StatusDescription
        {
            get
            {
                switch (StatusCode)
                {
                    case "0":
                        return Resources.Resource.RFormStatus_0;
                    case "1":
                        return Resources.Resource.RFormStatus_1;
                    case "2":
                        return Resources.Resource.RFormStatus_2;
                    case "3":
                        return Resources.Resource.RFormStatus_3;
                    case "4":
                        return Resources.Resource.RFormStatus_4;
                    case "5":
                        return Resources.Resource.RFormStatus_5;
                    case "6":
                        return Resources.Resource.RFormStatus_6;
                    case "7":
                        return Resources.Resource.RFormStatus_7;
                    case "8":
                        return Resources.Resource.RFormStatus_8;
                    case "9":
                        return Resources.Resource.RFormStatus_9;
                    default:
                        return "-";
                }
            }
        }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        [Display(Name = "Equipment", ResourceType = typeof(Resources.Resource))]
        public string Equipment
        {
            get
            {
                if (!string.IsNullOrEmpty(EquipmentID))
                {
                    if (string.IsNullOrEmpty(PartDescription))
                    {
                        return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                    }
                    else
                    {
                        return string.Format("{0}/{1}-{2}", EquipmentID, EquipmentName, PartDescription);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [Display(Name = "Subject", ResourceType = typeof(Resources.Resource))]
        public string Subject { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [Display(Name = "RepairFormType", ResourceType = typeof(Resources.Resource))]
        public string RepairFormType { get; set; }

        [Display(Name = "CreateUser", ResourceType = typeof(Resources.Resource))]
        public UserModel CreateUser { get; set; }

        public DateTime CreateTime { get; set; }

        [Display(Name = "CreateTime", ResourceType = typeof(Resources.Resource))]
        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public List<UserModel> JobManagerList { get; set; }

        public List<string> JobManagerIDList
        {
            get
            {
                if (JobManagerList != null && JobManagerList.Count > 0)
                {
                    return JobManagerList.Select(x => x.ID).ToList();
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        [Display(Name = "JobManager", ResourceType = typeof(Resources.Resource))]
        public string JobManagers
        {
            get
            {
                if (JobManagerList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var user in JobManagerList)
                    {
                        sb.Append(user.User);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [Display(Name = "RefuseReason", ResourceType = typeof(Resources.Resource))]
        public string RefuseReason { get; set; }

        public DateTime? JobTime { get; set; }

        [Display(Name = "JobTime", ResourceType = typeof(Resources.Resource))]
        public string JobTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(JobTime);
            }
        }

        public List<UserModel> JobUserList { get; set; }

        public List<string> JobUsers
        {
            get
            {
                return JobUserList.Select(x => x.ID).ToList();
            }
        }

        public DateTime? RealTakeJobTime { get; set; }

        public string RealTakeJobTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(RealTakeJobTime);
            }
        }

        public string RealTakeJobDateString { get; set; }

        public string RealTakeJobHour { get; set; }

        public List<SelectListItem> HourSelectItemList
        {
            get
            {
                var itemList = new List<SelectListItem>() 
                { 
                    Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                };

                for (int hour = 0; hour <= 23; hour++)
                {
                    var t = hour.ToString().PadLeft(2, '0');
                    itemList.Add(new SelectListItem()
                    {
                        Value = t,
                        Text = t
                    });
                }

                return itemList;
            }
        }

        public List<SelectListItem> RealTakeJobUserSelectItemList { get; set; }

        public List<SelectListItem> MinSelectItemList
        {
            get
            {
                var itemList = new List<SelectListItem>() 
                { 
                    Define.DefaultSelectListItem(Resources.Resource.SelectOne)
                };

                for (int min = 0; min <= 59; min++)
                {
                    var t = min.ToString().PadLeft(2, '0');

                    itemList.Add(new SelectListItem()
                    {
                        Value = t,
                        Text = t
                    });
                }

                return itemList;
            }
        }

        public string RealTakeJobMin { get; set; }

        public DateTime? TakeJobTime { get; set; }

        public DateTime? ClosedTime { get; set; }

        [DisplayName("結案時間")]
        public string ClosedTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(ClosedTime);
            }
        }

        public string RealTakeJobUserID { get; set; }

        [Display(Name = "TakeJobTime", ResourceType = typeof(Resources.Resource))]
        public string TakeJobTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(TakeJobTime);
            }
        }

        [Display(Name = "TakeJobUser", ResourceType = typeof(Resources.Resource))]
        public UserModel TakeJobUser { get; set; }

        public UserModel RealTakeJobUser { get; set; }

        [Display(Name = "JobRefuseReason", ResourceType = typeof(Resources.Resource))]
        public string JobRefuseReason { get; set; }

        public DateTime? EstBeginDate { get; set; }

        [Display(Name = "MaintenanceBeginDate", ResourceType = typeof(Resources.Resource))]
        public string EstBeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstBeginDate);
            }
        }

        public DateTime? EstEndDate { get; set; }

        [Display(Name = "MaintenanceEndDate", ResourceType = typeof(Resources.Resource))]
        public string EstEndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstEndDate);
            }
        }

        public List<ColumnModel> ColumnList { get; set; }

        public List<WorkingHourModel> WorkingHourList { get; set; }

        public List<MaterialModel> MaterialList { get; set; }

        public bool IsClosed { get; set; }

        public List<FlowLogModel> FlowLogList { get; set; }

        public List<UserModel> CurrentVerifyUserList { get; set; }

        public List<string> CurrentVerifyUserIDList
        {
            get
            {
                return CurrentVerifyUserList.Select(x => x.ID).ToList();
            }
        }

        public string CurrentVerifyUser
        {
            get
            {
                if (CurrentVerifyUserList != null && CurrentVerifyUserList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var user in CurrentVerifyUserList)
                    {
                        sb.Append(user.User);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        //public string CurrentVerifyUser
        //{
        //    get
        //    {
        //        if (IsClosed)
        //        {
        //            return string.Empty;
        //        }
        //        else
        //        {
        //            if (FlowLogList.Count > 0)
        //            {
        //                return FlowLogList.OrderBy(x => x.Seq).Select(x => x.User.ID).First();
        //            }
        //            else
        //            {
        //                return string.Empty;
        //            }
        //        }
        //    }
        //}

        public List<FileModel> FileList { get; set; }

        public DetailViewModel()
        {
            CreateUser = new UserModel();
            //JobManager = new UserModel();
            JobManagerList = new List<UserModel>();
            JobUserList = new List<UserModel>();
            TakeJobUser = new UserModel();
            ColumnList = new List<ColumnModel>();
            WorkingHourList = new List<WorkingHourModel>();
            MaterialList = new List<MaterialModel>();
            FlowLogList = new List<FlowLogModel>();
            FileList = new List<FileModel>();
            CurrentVerifyUserList = new List<UserModel>();
            RealTakeJobUserSelectItemList = new List<SelectListItem>() 
            {
                Define.DefaultSelectListItem(Resources.Resource.SelectOne)
            };
        }
    }
}
