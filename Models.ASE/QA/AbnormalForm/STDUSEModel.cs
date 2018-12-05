using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.AbnormalForm
{
    public class STDUSEModel
    {
        public string UniqueID { get; set; }

        public string CalNo { get; set; }

        public string CalibratorID { get; set; }

        public string CalibratorName { get; set; }

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

        public DateTime? LastCalibrateDate { get; set; }

        public string LastCalibrateDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(LastCalibrateDate);
            }
        }

        public DateTime? NextCalibrateDate { get; set; }

        public string NextCalibrateDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(NextCalibrateDate);
            }
        }
    }
}
