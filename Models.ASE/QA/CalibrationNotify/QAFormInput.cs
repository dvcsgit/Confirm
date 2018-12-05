using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationNotify
{
    public class QAFormInput
    {
       [DisplayName("類別")]
        public string CalibrateType { get; set; }

        [DisplayName("校驗負責單位")]
        public string CalibrateUnit { get; set; }

          [DisplayName("案件類別")]
        public string CaseType { get; set; }

       [DisplayName("校驗負責人員")]
        public string CalibratorID { get; set; }

        [DisplayName("預計校驗日期")]
        public string EstCalibrateDateString { get; set; }

        public DateTime? EstCalibrateDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(EstCalibrateDateString);
            }
        }

        [DisplayName("簽核意見")]
        public string Comment { get; set; }
    }
}
