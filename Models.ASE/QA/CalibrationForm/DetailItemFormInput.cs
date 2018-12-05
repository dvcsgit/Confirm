using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
    public class DetailItemFormInput
    {
        //[Display(Name = "CalibrateDate", ResourceType = typeof(Resources.Resource))]
        //[Required(ErrorMessageResourceName = "CalibrateDateRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        //public string CalibrateDateString { get; set; }

        //public DateTime? CalibrateDate
        //{
        //    get
        //    {
        //        return DateTimeHelper.DateStringWithSeperator2DateTime(CalibrateDateString);
        //    }
        //}

        [Display(Name = "ReadingValue", ResourceType = typeof(Resources.Resource))]
        //[Required(ErrorMessageResourceName = "ReadingValueRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public decimal? ReadingValue { get; set; }

        public decimal? Standard { get; set; }
    }
}
