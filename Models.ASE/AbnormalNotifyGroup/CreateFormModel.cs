using Models.ASE.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.AbnormalNotifyGroup
{
    public class CreateFormModel
    {
        public FormInput FormInput { get; set; }

        public List<ASEUserModel> UserList { get; set; }

        public List<ASEUserModel> CCUserList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            UserList = new List<ASEUserModel>();
            CCUserList = new List<ASEUserModel>();
        }
    }
}
