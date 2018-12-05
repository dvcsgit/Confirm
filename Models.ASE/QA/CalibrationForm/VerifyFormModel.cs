using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
   public class VerifyFormModel
    {
        public string UniqueID { get; set; }

        public FormViewModel FormViewModel { get; set; }

        public VerifyFormInput FormInput { get; set; }

        public VerifyFormModel()
        {
            FormViewModel = new FormViewModel();
            FormInput = new VerifyFormInput();
        }
    }
}
