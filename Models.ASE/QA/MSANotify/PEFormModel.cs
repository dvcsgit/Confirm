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
    public class PEFormModel
    {
        public string UniqueID { get; set; }

        public FormViewModel FormViewModel { get; set; }

        public List<SelectListItem> MSAStationSelectItemList { get; set; }

        public List<SelectListItem> MSAIchiSelectItemList { get; set; }

        public List<MSAIchiModel> MSAIchiList { get; set; }

        public PEFormInput FormInput { get; set; }

        public List<VerifyCommentModel> VerifyCommentList { get; set; }

        public PEFormModel()
        {
            FormViewModel = new FormViewModel();
            FormInput = new PEFormInput();
            MSAStationSelectItemList = new List<SelectListItem>();
            MSAIchiSelectItemList = new List<SelectListItem>();
            MSAIchiList = new List<MSAIchiModel>();
            VerifyCommentList = new List<VerifyCommentModel>();
        }
    }
}
