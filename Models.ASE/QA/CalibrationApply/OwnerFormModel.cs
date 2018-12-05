using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.QA.CalibrationApply
{
   public class OwnerFormModel
    {
        public string UniqueID { get; set; }

        public FormViewModel FormViewModel { get; set; }

        public OwnerFormInput FormInput { get; set; }

        public List<VerifyCommentModel> VerifyCommentList { get; set; }

        public OwnerFormModel()
        {
            FormViewModel = new FormViewModel();
            FormInput = new OwnerFormInput();
            VerifyCommentList = new List<VerifyCommentModel>();
        }
    }
}
