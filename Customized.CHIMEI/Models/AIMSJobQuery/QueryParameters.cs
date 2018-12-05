using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.CHIMEI.Models.AIMSJobQuery
{
    public class QueryParameters
    {
        public string OrganizationUniqueID { get; set; }

        public string VHNO { get; set; }

        public string Cycle { get; set; }

        public string MotorType { get; set; }

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
                return DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString);
            }
        }
    }
}
