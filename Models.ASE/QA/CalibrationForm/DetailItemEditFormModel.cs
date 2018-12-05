using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.ASE.QA.CalibrationForm
{
    public class DetailItemEditFormModel
    {
        public string CalibrationFormUniqueID { get; set; }

        public int Seq { get; set; }

        [Display(Name = "Characteristic", ResourceType = typeof(Resources.Resource))]
        public string Characteristic { get; set; }

        [Display(Name = "UsingRange", ResourceType = typeof(Resources.Resource))]
        public string UsingRange { get; set; }

        [Display(Name = "UsingRangeTolerance", ResourceType = typeof(Resources.Resource))]
        public string RangeTolerance { get; set; }

        [Display(Name = "CalibrationPoint", ResourceType = typeof(Resources.Resource))]
        public string CalibrationPoint { get; set; }

        [Display(Name = "Tolerance", ResourceType = typeof(Resources.Resource))]
        public string Tolerance { get; set; }

        public DetailItemFormInput FormInput { get; set; }

        public List<PhotoModel> PhotoList { get; set; }

        public DetailItemEditFormModel()
        {
            FormInput = new DetailItemFormInput();
            PhotoList = new List<PhotoModel>();
        }
    }
}
