using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.MaintanceSchedule
{
    public class GridItem
    {
        public string Organization { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string EquipmentPart { get; set; }

        public string MRoute { get; set; }

        public string CycleMode { get; set; }

        public string MJobBeginDate { get; set; }

        public string MJobEndDate { get; set; }

        public string MFormBeginDate { get; set; }
    }
}
