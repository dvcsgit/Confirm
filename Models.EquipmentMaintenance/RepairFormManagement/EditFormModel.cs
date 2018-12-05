using Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;
using System.Linq;
using System.Web.Mvc;
using System.Text;

namespace Models.EquipmentMaintenance.RepairFormManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string AncestorOrganizationUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "MaintenanceOrganization", ResourceType = typeof(Resources.Resource))]
        public string MaintenanceOrganizationFullDescription { get; set; }

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
                switch (Status)
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

        public string EquipmentUniqueID { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartUniqueID { get; set; }

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

        public DateTime? JobTime { get; set; }

        [Display(Name = "JobTime", ResourceType = typeof(Resources.Resource))]
        public string JobTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(JobTime);
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

        public List<UserModel> JobUserList { get; set; }

        public List<string> JobUsers
        {
            get
            {
                return JobUserList.Select(x => x.ID).ToList();
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

        [Display(Name = "TakeJobUser", ResourceType = typeof(Resources.Resource))]
        public UserModel TakeJobUser { get; set; }

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

        public string RefuseReason { get; set; }

        public string JobRefuseReason { get; set; }

        public bool IsClosed { get; set; }

        public DateTime? RealTakeJobTime { get; set; }

        public string RealTakeJobTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(RealTakeJobTime);
            }
        }

        public UserModel RealTakeJobUser { get; set; }

        public string ClosedDateString { get; set; }

        public string ClosedHour { get; set; }

        public string ClosedMin { get; set; }

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

        public List<ColumnModel> ColumnList { get; set; }

        public List<WorkingHourModel> WorkingHourList { get; set; }

        public List<MaterialModel> MaterialList { get; set; }

        public List<FileModel> FileList { get; set; }

        public EditFormModel()
        {
            CreateUser = new UserModel();
            //JobManager = new UserModel();
            JobManagerList = new List<UserModel>();
            TakeJobUser = new UserModel();
            JobUserList = new List<UserModel>();
            ColumnList = new List<ColumnModel>();
            WorkingHourList = new List<WorkingHourModel>();
            MaterialList = new List<MaterialModel>();
            FileList = new List<FileModel>();
        }
    }
}
