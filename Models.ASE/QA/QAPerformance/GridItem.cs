using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.QAPerformance
{
    public class GridItem
    {
        public string UserID { get; set; }

        public string UserName { get; set; }

        [DisplayName("人員")]
        public string User
        {
            get
            {
                return string.Format("{0}/{1}", UserID, UserName);
            }
        }

        [DisplayName("核簽數量(校驗申請單)")]
        public int CalibrationApplyCount { get; set; }

        [DisplayName("核簽數量(校驗通知單)")]
        public int CalibrationNotifyCount { get; set; }

        [DisplayName("核簽數量(校驗執行單)")]
        public int CalibrationFormCount { get; set; }

        [DisplayName("核簽數量(MSA通知單)")]
        public int MSANotifyCount { get; set; }

        [DisplayName("核簽數量(MSA執行單)")]
        public int MSAFormCount { get; set; }

        [DisplayName("核簽數量(異動申請單)")]
        public int ChangeFormCount { get; set; }

        [DisplayName("核簽數量(異常通知單)")]
        public int AbnormalFormCount { get; set; }

        [DisplayName("核簽數量(小計)")]
        public int VerifyCount
        {
            get
            {
                return CalibrationApplyCount + CalibrationNotifyCount + CalibrationFormCount + MSANotifyCount + MSAFormCount + ChangeFormCount + AbnormalFormCount;
            }
        }

        [DisplayName("校驗(收件次數)")]
        public int Step1Count { get; set; }

        [DisplayName("校驗(送件次數)")]
        public int Step2Count { get; set; }

        [DisplayName("校驗(回件次數)")]
        public int Step3Count { get; set; }

        [DisplayName("校驗(發件次數)")]
        public int Step4Count { get; set; }

        [DisplayName("校驗(執行數量)")]
        public int CalibrationCount { get; set; }
    }
}


