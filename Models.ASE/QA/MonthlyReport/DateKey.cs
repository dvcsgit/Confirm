using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MonthlyReport
{
    public class DateKey
    {
        public string Display
        {
            get
            {
                return string.Format("{0}'{1}", BeginDate.Month.ToString().PadLeft(2, '0'), BeginDate.Year.ToString().Substring(2)); 
            }
        }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
