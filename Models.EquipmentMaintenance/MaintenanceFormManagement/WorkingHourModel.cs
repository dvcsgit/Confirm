using Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class WorkingHourModel
    {
        public int Seq { get; set; }

        public string BeginDate { get; set; }

        public string BeginDateString
        {
            get
            {
                return DateTimeHelper.DateString2DateStringWithSeparator(BeginDate);
            }
        }

        public string EndDate { get; set; }

        public string EndDateString
        {
            get
            {
                return DateTimeHelper.DateString2DateStringWithSeparator(EndDate);
            }
        }

        public double WorkingHour { get; set; }

        public UserModel User { get; set; }

        public WorkingHourModel()
        {
            User = new UserModel();
        }
    }
}
