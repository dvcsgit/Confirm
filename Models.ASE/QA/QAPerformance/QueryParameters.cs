using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.QAPerformance
{
    public class QueryParameters
    {
        [DisplayName("日期(起)")]
        public string BeginDateString { get; set; }

        public DateTime? BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(BeginDateString);
            }
        }

        [DisplayName("日期(迄)")]
        public string EndDateString { get; set; }

        public DateTime? EndDate
        {
            get
            {
                var endDate = DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString);

                if (endDate.HasValue)
                {
                    return DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString).Value.AddDays(1);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
