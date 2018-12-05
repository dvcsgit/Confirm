using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.EquipmentManagement
{
    public class CALFormModel
    {
        public string UniqueID { get; set; }

        public EquipmentModel Equipment { get; set; }

        public CALFormInput FormInput { get; set; }

        public CALFormModel()
        {
            Equipment = new EquipmentModel();
            FormInput = new CALFormInput();
        }
    }
}
