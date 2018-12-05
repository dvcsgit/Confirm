using System;
using Utility;

namespace Customized.CHIMEI.Models.TrendQuery
{
    public class QueryFormModel
    {
        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            Parameters = new QueryParameters()
            {
                BeginDateString = DateTimeHelper.DateTime2DateStringWithSeperator(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1)),
                EndDateString = DateTimeHelper.DateTime2DateStringWithSeperator(DateTime.Today)
            };
        }
    }
}
