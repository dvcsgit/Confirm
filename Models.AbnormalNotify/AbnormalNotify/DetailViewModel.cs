using Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.AbnormalNotify.AbnormalNotify
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

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

        [DisplayName("復原說明")]
        public string RecoveryDescription { get; set; }

        [DisplayName("影響區域(產線單位)")]
        public string EffectArea { get; set; }

        [DisplayName("影響系統(FAC系統)")]
        public string EffectSystem { get; set; }

        [DisplayName("損失金額")]
        public string Cost { get; set; }

        public List<UserModel> NotifyUserList { get; set; }

        public List<UserModel> NotifyCCUserList { get; set; }

        public List<FileModel> FileList { get; set; }

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

        public List<LogModel> LogList { get; set; }

        public DetailViewModel()
        {
            NotifyUserList = new List<UserModel>();
            NotifyCCUserList = new List<UserModel>();
            FileList = new List<FileModel>();
            LogList = new List<LogModel>();
            GroupList = new List<string>();
        }
    }
}
