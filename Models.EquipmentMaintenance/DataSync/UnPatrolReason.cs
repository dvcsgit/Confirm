using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.DataSync
{
    public class UnPatrolReason
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public DateTime LastModifyTime { get; set; }
    }
}
