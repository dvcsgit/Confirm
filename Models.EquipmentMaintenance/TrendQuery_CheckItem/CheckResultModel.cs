using System;
using Utility;

namespace Models.EquipmentMaintenance.TrendQuery_CheckItem
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
                //return DateTimeHelper.DateTime2DateTimeStringWithSeperator(DateTimeHelper.DateTimeString2DateTime(CheckDate, CheckTime));
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
