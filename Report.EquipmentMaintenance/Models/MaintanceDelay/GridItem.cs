using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.MaintanceDelay
{
    public class GridItem
    {
        public string VHNO { get; set; }

        public string MrouteDescription { get; set; }

        public string MCycle { get; set; }

        public string CycleBeginDate { get; set; }

        public string CycleEndDate { get; set; }

        public string MFormUserID { get; set; }

        public string MFormBeginDate { get; set; }

        public string Status { get; set; }

        public int DelayDays { get; set; }
    }
}
