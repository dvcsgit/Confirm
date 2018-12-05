using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.ChangeForm
{
    public class QueryParameters
    {
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
                    return DateTimeHelper.DateStringWithSeperator2DateTime(EndDateString).Value.AddDays(1);
                }
                else
                {
                    return null;
                }
            }
        }

        [Display(Name = "Status", ResourceType = typeof(Resources.Resource))]
        public string Status { get; set; }

        [Display(Name = "ChangeType", ResourceType = typeof(Resources.Resource))]
        public string ChangeType { get; set; }

        [Display(Name = "VHNO", ResourceType = typeof(Resources.Resource))]
        public string VHNO { get; set; }

        public string CALNO { get; set; }

        public string OwnerID { get; set; }
    }
}
