using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.QA.CalibrationNotify
{
   public class ManagerFormModel
    {
        public string UniqueID { get; set; }

        public FormViewModel FormViewModel { get; set; }

        public ManagerFormInput FormInput { get; set; }

        public List<VerifyCommentModel> VerifyCommentList { get; set; }

        public ManagerFormModel()
        {
            FormViewModel = new FormViewModel();
            FormInput = new ManagerFormInput();
            VerifyCommentList = new List<VerifyCommentModel>();
        }
    }
}
