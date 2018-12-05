using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.MonthlyReport
{
    public class JobModel
    {
        public string UniqueID { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int CycleCount { get; set; }

        public string CycleMode { get; set; }

        public string Description { get; set; }
    }
}
