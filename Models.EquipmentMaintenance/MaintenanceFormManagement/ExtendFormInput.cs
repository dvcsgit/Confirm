using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class ExtendFormInput
    {
        public DateTime? NBeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(NBeginDateString);
            }
        }

        public string NBeginDateString { get; set; }

        public DateTime? NEndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateTime(NEndDateString);
            }
        }

        public string NEndDateString { get; set; }

        public string Reason { get; set; }
    }
}
