using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.Calendar
{
    public class Event
    {
        public string id { get; set; }
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public bool allDay { get; set; }
        public string color { get; set; }

        public bool IsRepairForm { get; set; }

        public bool IsMaintenanceForm { get; set; }

        public string VHNO { get; set; }
    }
}
