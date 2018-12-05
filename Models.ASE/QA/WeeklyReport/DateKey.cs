using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.WeeklyReport
{
    public class DateKey
    {
        public string Display
        {
            get
            {
                DateTimeFormatInfo dateTimeInfo = DateTimeFormatInfo.CurrentInfo;

                var week = dateTimeInfo.Calendar.GetWeekOfYear(BeginDate, dateTimeInfo.CalendarWeekRule, dateTimeInfo.FirstDayOfWeek);

                return string.Format("W{0}'{1}", week.ToString().PadLeft(2, '0'), BeginDate.Year.ToString().Substring(2));
            }
        }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
