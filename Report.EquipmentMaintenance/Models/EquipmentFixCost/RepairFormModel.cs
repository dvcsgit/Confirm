using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.EquipmentFixCost
{
    public class RepairFormModel
    {
        public DateTime CreateTime { get; set; }

        public DateTime ClosedTime { get; set; }

        public double Duration
        {
            get
            {
                return (ClosedTime - CreateTime).TotalHours;
            }
        }
    }
}
