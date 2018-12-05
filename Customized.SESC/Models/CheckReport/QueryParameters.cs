using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Customized.SESC.Models.CheckReport
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
