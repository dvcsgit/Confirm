using Models.ASE.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.AbnormalNotifyGroup
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public string GroupType { get; set; }

        [DisplayName("通知群組類別")]
        public string GroupTypeDisplay
        {
            get
            {
                if (GroupType == "1")
                {
                    return "異常通報";
                }
                else
                {
                    return "災損填報";
                }
            }
        }

        [DisplayName("通知群組名稱")]
        public string Description { get; set; }

        public bool CanDelete { get; set; }

        public List<ASEUserModel> UserList { get; set; }

        public List<ASEUserModel> CCUserList { get; set; }

        public DetailViewModel()
        {
            UserList = new List<ASEUserModel>();
            CCUserList = new List<ASEUserModel>();
        }
    }
}
