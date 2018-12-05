using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.DailyReport
{
    public class DateKey
    {
        public string Display
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(Date);
            }
        }

        public DateTime Date { get; set; }
    }
}
