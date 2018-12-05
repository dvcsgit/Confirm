using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.AbnormalNotify_v2
{
    public class QueryParameters
    {
        public string OccurBeginDateString { get; set; }

        public string OccurBeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(OccurBeginDateString);
            }
        }

        public string OccurEndDateString { get; set; }

        public string OccurEndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(OccurEndDateString);
            }
        }

        [Display(Name = "Status", ResourceType = typeof(Resources.Resource))]
        public string Status { get; set; }

        [DisplayName("異常主旨")]
        public string Subject { get; set; }

        [DisplayName("負責單位")]
        public string ResOrganizationUniqueID { get; set; }

        public string ClosedBeginDateString { get; set; }

        public DateTime? ClosedBeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(ClosedBeginDateString);
            }
        }

        public string ClosedEndDateString { get; set; }

        public DateTime? ClosedEndDate
        {
            get
            {
                var endDate = DateTimeHelper.DateStringWithSeperator2DateTime(ClosedEndDateString);

                if (endDate.HasValue)
                {
                    return endDate.Value.AddDays(1);
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
