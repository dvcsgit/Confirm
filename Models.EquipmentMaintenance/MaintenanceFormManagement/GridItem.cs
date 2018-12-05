using Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        [DisplayName("組織")]
        public string OrganizationDescription { get; set; }

        [DisplayName("單號")]
        public string VHNO { get; set; }

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

        [DisplayName("狀態")]
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

        [DisplayName("主旨")]
        public string Subject { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        [DisplayName("設備")]
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

        [DisplayName("保養週期(起)")]
        public string CycleBeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(CycleBeginDate);
            }
        }

        public DateTime CycleEndDate { get; set; }

        [DisplayName("保養週期(迄)")]
        public string CycleEndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(CycleEndDate);
            }
        }

        public DateTime EstBeginDate { get; set; }

        [DisplayName("預計保養日期(起)")]
        public string EstBeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstBeginDate);
            }
        }

        public DateTime EstEndDate { get; set; }

        [DisplayName("預計保養日期(迄)")]
        public string EstEndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EstEndDate);
            }
        }

        public DateTime? BeginDate { get; set; }

        [DisplayName("實際保養日期(起)")]
        public string BeginDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(BeginDate);
            }
        }

        public DateTime? EndDate { get; set; }

        [DisplayName("實際保養日期(迄)")]
        public string EndDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(EndDate);
            }
        }

        public List<UserModel> JobUserList { get; set; }

        [DisplayName("派工人員")]
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

        [DisplayName("立案時間")]
        public string CreateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(CreateTime);
            }
        }

        public string TakeJobUserID { get; set; }

        public string TakeJobUserName { get; set; }

        [DisplayName("接案人員")]
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

        [DisplayName("接案時間")]
        public string TakeJobTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(TakeJobTime);
            }
        }

        public List<UserModel> MaintenanceUserList { get; set; }

        [DisplayName("保養人員")]
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

        public string CurrentVerifyUserID { get; set; }

        public string CurrentExtendVerifyUserID { get; set; }

        public GridItem()
        {
            JobUserList = new List<UserModel>();
            MaintenanceUserList = new List<UserModel>();
        }
    }
}
