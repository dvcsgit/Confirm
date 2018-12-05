using Models.ASE.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.ASE.AbnormalNotify
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

        public List<SelectListItem> ResponsibleOrganizationSelectItemList { get; set; }

        public Dictionary<string, string> GroupList { get; set; }

        public List<FileModel> FileList { get; set; }

        public List<ASEUserModel> NotifyUserList { get; set; }

        public List<ASEUserModel> NotifyCCUserList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            FileList = new List<FileModel>();
            ResponsibleOrganizationSelectItemList = new List<SelectListItem>();
            GroupList = new Dictionary<string, string>();
            NotifyUserList = new List<ASEUserModel>();
            NotifyCCUserList = new List<ASEUserModel>();
        }
    }
}
