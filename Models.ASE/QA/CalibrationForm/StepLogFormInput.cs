using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
    public class StepLogFormInput
    {
        public string OwnerID { get; set; }

        public string QAID { get; set; }

        public string DateString { get; set; }

        public DateTime Time
        {
            get
            {
                return DateTimeHelper.DateTimeString2DateTime(DateTimeHelper.DateStringWithSeperator2DateString(DateString), string.Format("{0}{1}00", Hour, Minute)).Value;
            }
        }

        public string Hour { get; set; }

        public string Minute { get; set; }
    }
}
