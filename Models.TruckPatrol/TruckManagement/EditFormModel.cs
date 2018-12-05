using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TruckPatrol.TruckManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string TruckType { get; set; }

        public FormInput FormInput { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
        }
    }
}
