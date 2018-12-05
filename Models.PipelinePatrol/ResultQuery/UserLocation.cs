using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.ResultQuery
{
    public class UserLocation
    {
        public string Date { get; set; }

        public string Time { get; set; }

        public string TimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(Date, Time));
            }
        }

        public double LAT { get; set; }

        public double LNG { get; set; }
    }
}
