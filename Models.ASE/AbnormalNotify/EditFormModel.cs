using Models.ASE.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.AbnormalNotify
{
    public class EditFormModel
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

        public string ClosedRemark { get; set; }

        public RepairFormModel RepairForm { get; set; }

        public FormInput FormInput { get; set; }

        public string OccurTimePickerValue
        {
            get
            {
                if (FormInput != null && !string.IsNullOrEmpty(FormInput.OccurTime))
                {
                    return FormInput.OccurTime.Substring(0, 2) + ":" + FormInput.OccurTime.Substring(2);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string RecoveryTimePickerValue
        {
            get
            {
                if (FormInput != null && !string.IsNullOrEmpty(FormInput.RecoveryTime))
                {
                    return FormInput.RecoveryTime.Substring(0, 2) + ":" + FormInput.RecoveryTime.Substring(2);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [DisplayName("後續追蹤負責單位")]
        public string ResponsibleOrganization { get; set; }

        public Dictionary<string, string> GroupList { get; set; }

        public List<string> FormGroupList { get; set; }

        public List<FileModel> FileList { get; set; }

        public List<ASEUserModel> NotifyUserList { get; set; }

        public List<ASEUserModel> NotifyCCUserList { get; set; }

        public List<LogModel> LogList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            FileList = new List<FileModel>();
            GroupList = new Dictionary<string, string>();
            FormGroupList = new List<string>();
            NotifyUserList = new List<ASEUserModel>();
            NotifyCCUserList = new List<ASEUserModel>();
            LogList = new List<LogModel>();
            RepairForm = new RepairFormModel();
        }
    }
}
