using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.RepairFormManagement
{
    public class RFormSubject
    {
        public string UniqueID { get; set; }

        public string AncestorOrganizationUniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }
    }
}
