using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.InventoryManagerManagement
{
    public class EditFormModel
    {
        public string OrganizationDescription { get; set; }

        public List<string> ManagerList { get; set; }

        public FormInput FormInput { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            ManagerList = new List<string>();
        }
    }
}
