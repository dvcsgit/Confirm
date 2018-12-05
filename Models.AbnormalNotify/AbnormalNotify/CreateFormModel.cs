using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.AbnormalNotify.AbnormalNotify
{
    public class CreateFormModel
    {
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

        public Dictionary<string, string> GroupList { get; set; }

        public List<FileModel> FileList { get; set; }

        public List<UserModel> NotifyUserList { get; set; }

        public List<UserModel> NotifyCCUserList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            FileList = new List<FileModel>();
            GroupList = new Dictionary<string, string>();
            NotifyUserList = new List<UserModel>();
            NotifyCCUserList = new List<UserModel>();
        }
    }
}
