using Models.ASE.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.AbnormalNotify_v2
{
   public  class ClosedFormModel
    {
       public string UniqueID { get; set; }

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        public string Status { get; set; }

        [Display(Name = "Status", ResourceType = typeof(Resources.Resource))]
        public string StatusDescription
        {
            get
            {
                if (Status == "0")
                {
                    return Resources.Resource.AbnormalNotifyStatus_0;
                }
                else if (Status == "1")
                {
                    return Resources.Resource.AbnormalNotifyStatus_1;
                }
                else if (Status == "2")
                {
                    return Resources.Resource.AbnormalNotifyStatus_2;
                }
                else if (Status == "3")
                {
                    return Resources.Resource.AbnormalNotifyStatus_3;
                }
                else
                {
                    return "-";
                }
            }
        }

        public DateTime? OccurTime { get; set; }

        [DisplayName("發生時間 (When)")]
        public string OccurTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(OccurTime);
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

        public string CreateUserID { get; set; }

        public string CreateUserName { get; set; }

        [Display(Name = "CreateUser", ResourceType = typeof(Resources.Resource))]
        public string CreateUser
        {
            get
            {
                if (!string.IsNullOrEmpty(CreateUserName))
                {
                    return string.Format("{0}/{1}", CreateUserID, CreateUserName);
                }
                else
                {
                    return CreateUserID;
                }
            }
        }

        [DisplayName("後續追蹤負責單位")]
        public string ResponsibleOrganization { get; set; }

        [DisplayName("聯絡人員 (Who)")]
        public string Contact { get; set; }

        [DisplayName("MVPN")]
        public string Mvpn { get; set; }

        [DisplayName("異常主旨 (What)")]
        public string Subject { get; set; }

        [DisplayName("地點 (Where)")]
        public string Location { get; set; }

        [DisplayName("異常原因 (Why)")]
        public string Description { get; set; }

        [DisplayName("緊急對策 (How)")]
        public string HandlingDescription { get; set; }

        public DateTime? RecoveryTime { get; set; }

        [DisplayName("復原時間")]
        public string RecoveryTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(RecoveryTime);
            }
        }

        public List<string> ResponsibleOrganizationList { get; set; }

        [DisplayName("復原說明")]
        public string RecoveryDescription { get; set; }

        [DisplayName("影響區域(產線單位)")]
        public string EffectArea { get; set; }

        [DisplayName("影響系統(FAC系統)")]
        public string EffectSystem { get; set; }

        [DisplayName("損失金額")]
        public string Cost { get; set; }

        public List<ASEUserModel> NotifyUserList { get; set; }

        public List<ASEUserModel> NotifyCCUserList { get; set; }

        public List<FileModel> FileList { get; set; }

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

        [DisplayName("通知群組")]
        public string Groups
        {
            get
            {
                if (GroupList != null && GroupList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var group in GroupList)
                    {
                        sb.Append(group);
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

        public List<string> GroupList { get; set; }

        public RepairFormModel RepairForm { get; set; }

        public List<LogModel> LogList { get; set; }

        public ClosedFormInput FormInput { get; set; }

        public ClosedFormModel()
        {
            NotifyUserList = new List<ASEUserModel>();
            NotifyCCUserList = new List<ASEUserModel>();
            FileList = new List<FileModel>();
            RepairForm = new RepairFormModel();
            ResponsibleOrganizationList = new List<string>();
            LogList = new List<LogModel>();
            GroupList = new List<string>();
        }
    }
}
