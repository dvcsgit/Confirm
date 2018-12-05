using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CalibrationForm
{
    public class QRCodeItem
    {
         //GuidFileName
        public string SN { get; set; }
        public string CALDate { get; set; }
        public string DueDate { get; set; }
        public string Sign { get; set; }
        public bool IsFailed { get; set; }
    }
}
