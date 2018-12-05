using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
    public class TakeJobLogModel
    {
        public decimal Seq { get; set; }

        public DateTime Time { get; set; }

        public string TimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(Time);
            }
        }

        public string CalibratorID { get; set; }

        public string CalibratorName { get; set; }

        [Display(Name = "Calibrator", ResourceType = typeof(Resources.Resource))]
        public string Calibrator
        {
            get
            {
                if (!string.IsNullOrEmpty(CalibratorName))
                {
                    return string.Format("{0}/{1}", CalibratorID, CalibratorName);
                }
                else
                {
                    return CalibratorID;
                }
            }
        }
    }
}
