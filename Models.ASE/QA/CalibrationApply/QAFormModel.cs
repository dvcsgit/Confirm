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
   public class QAFormModel
    {
        public string UniqueID { get; set; }

        public FormViewModel FormViewModel { get; set; }

        public List<SelectListItem> CalibratorSelectItemList { get; set; }

        public QAFormInput FormInput { get; set; }

        public List<VerifyCommentModel> VerifyCommentList { get; set; }

        public QAFormModel()
        {
            FormViewModel = new FormViewModel();
            CalibratorSelectItemList = new List<SelectListItem>();
            FormInput = new QAFormInput();
            VerifyCommentList = new List<VerifyCommentModel>();
        }
    }
}
