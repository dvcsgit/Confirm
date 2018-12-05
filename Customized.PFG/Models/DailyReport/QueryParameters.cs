using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Utility;

namespace Customized.PFG.Models.DailyReport
{
    public class QueryParameters
    {
        public string RouteUniqueID { get; set; }

        [Display(Name = "CheckDate", ResourceType = typeof(Resources.Resource))]
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
