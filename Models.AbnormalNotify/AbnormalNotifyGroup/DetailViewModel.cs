using Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.AbnormalNotify.AbnormalNotifyGroup
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [DisplayName("通知群組名稱")]
        public string Description { get; set; }

        public bool CanDelete { get; set; }

        public List<UserModel> UserList { get; set; }

        public List<UserModel> CCUserList { get; set; }

        public DetailViewModel()
        {
            UserList = new List<UserModel>();
            CCUserList = new List<UserModel>();
        }
    }
}
