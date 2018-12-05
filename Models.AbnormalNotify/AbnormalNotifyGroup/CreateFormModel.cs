using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.AbnormalNotify.AbnormalNotifyGroup
{
    public class CreateFormModel
    {
        public FormInput FormInput { get; set; }

        public List<UserModel> UserList { get; set; }

        public List<UserModel> CCUserList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            UserList = new List<UserModel>();
            CCUserList = new List<UserModel>();
        }
    }
}
