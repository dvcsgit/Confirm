using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.PFG.CN.Models.AbnormalHanding
{
    public class QueryParameters
    {
        public string RouteUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "CheckDate", ResourceType = typeof(Resources.Resource))]
        public string DateString { get; set; }

        public string CheckDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(DateString);
            }
        }
        [Display(Name = "BeginDate", ResourceType = typeof(Resources.Resource))]
        public string BeginDateString { get; set; }

        public string BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(BeginDateString);
            }
        }

        [Display(Name = "EndDate", ResourceType = typeof(Resources.Resource))]
        public string EndDateString { get; set; }

        public string EndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(EndDateString);
            }
        }
    }
}
