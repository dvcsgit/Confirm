using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.Inventory
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public FormInput FormInput { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
        }
    }
}
