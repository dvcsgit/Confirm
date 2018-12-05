using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.AbnormalForm
{
    public class DetailItem
    {
        public decimal Seq { get; set; }

        public string Standard { get; set; }

        public DateTime? CalibrateDate { get; set; }

        public string CalibrateDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(CalibrateDate);
            }
        }

        public string Characteristic { get; set; }

        public string UsingRange { get; set; }

        public string CalibrationPoint { get; set; }

        public string Tolerance { get; set; }

        public double? ReadingValue { get; set; }
    }
}
