using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.FPTC.Models.DispatchQuery
{
    public class QueryParameters
    {
        public string CompanyID { get; set; }

        public string DepartmentID { get; set; }

        public string CarNo { get; set; }

        public string BeginDateString { get; set; }

        public DateTime? BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(BeginDateString);
            }
        }

        public string EndDateString { get; set; }

        public DateTime? EndDate
        {
            get
            {
                var endDate = DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString);

                if (endDate.HasValue)
                {
                    return endDate.Value.AddDays(1);
                }
                else
                {
                    return endDate;
                }
            }
        }

        public string IsChecked { get; set; }
    }
}
