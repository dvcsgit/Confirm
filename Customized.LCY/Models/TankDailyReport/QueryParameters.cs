using System;
using Utility;

namespace Customized.LCY.Models.TankDailyReport
{
    public class QueryParameters
    {
        public string DateString { get; set; }

        public string Date
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(DateString);
            }
        }
    }
}
