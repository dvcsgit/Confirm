using Models.ASE.QA.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.EquipmentManagement
{
    public class MSAFormModel
    {
        public string UniqueID { get; set; }

        public EquipmentModel Equipment { get; set; }

        public MSAFormInput FormInput { get; set; }

        public MSAFormModel()
        {
            Equipment = new EquipmentModel();
            FormInput = new MSAFormInput();
        }
    }
}
