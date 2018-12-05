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
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public FormInput FormInput { get; set; }

        public List<UserModel> UserList { get; set; }

        public List<UserModel> CCUserList { get; set; }

        public EditFormModel()
        {
            UserList = new List<UserModel>();
            CCUserList = new List<UserModel>();
        }
    }
}
