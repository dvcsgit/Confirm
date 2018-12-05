using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.CHIMEI.Models.TrendQuery
{
    public class CheckResultModel
    {
        public string CheckDate { get; set; }

        public string CheckTime { get; set; }

        public DateTime CheckDateTime
        {
            get
            {
                return DateTimeHelper.DateTimeString2DateTime(CheckDate, CheckTime).Value;
            }
        }

        public double? NetValue { get; set; }

        public double? Value { get; set; }

        public string Result
        {
            get
            {
                if (NetValue.HasValue)
                {
                    return NetValue.Value.ToString();
                }
                else
                {
                    return Value.Value.ToString();
                }
            }
        }
    }
}
