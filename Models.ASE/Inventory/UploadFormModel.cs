using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.Inventory
{
    public class UploadFormModel
    {
        public string InitialMessage { get; set; }

        public UploadFormInput FormInput { get; set; }

        public UploadFormModel()
        {
            FormInput = new UploadFormInput();
        }
    }
}
