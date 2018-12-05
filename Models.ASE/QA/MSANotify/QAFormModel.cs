using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Utility;

namespace Models.ASE.QA.MSANotify
{
   public class QAFormModel
    {
        public string UniqueID { get; set; }

        public FormViewModel FormViewModel { get; set; }

        public QAFormInput FormInput { get; set; }

        public List<VerifyCommentModel> VerifyCommentList { get; set; }

        public QAFormModel()
        {
            FormViewModel = new FormViewModel();
            FormInput = new QAFormInput();
            VerifyCommentList = new List<VerifyCommentModel>();
        }
    }
}
