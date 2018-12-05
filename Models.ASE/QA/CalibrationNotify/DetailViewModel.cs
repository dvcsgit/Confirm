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
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public FormViewModel FormViewModel { get; set; }

        public DetailViewModel()
        {
            FormViewModel = new FormViewModel();
        }
    }
}