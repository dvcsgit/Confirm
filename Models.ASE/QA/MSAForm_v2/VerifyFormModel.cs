using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSAForm_v2
{
   public class VerifyFormModel
    {
        public string UniqueID { get; set; }

        public DetailViewModel FormViewModel { get; set; }

        public VerifyFormInput FormInput { get; set; }

        public VerifyFormModel()
        {
            FormViewModel = new DetailViewModel();
            FormInput = new VerifyFormInput();
        }
    }
}
