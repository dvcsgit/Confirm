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
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

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

        public Dictionary<string, string> GroupList { get; set; }

        public List<string> FormGroupList { get; set; }

        public List<FileModel> FileList { get; set; }

        public List<UserModel> NotifyUserList { get; set; }

        public List<UserModel> NotifyCCUserList { get; set; }

        public List<LogModel> LogList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            FileList = new List<FileModel>();
            GroupList = new Dictionary<string, string>();
            FormGroupList = new List<string>();
            NotifyUserList = new List<UserModel>();
            NotifyCCUserList = new List<UserModel>();
            LogList = new List<LogModel>();
        }
    }
}
