using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.EquipmentResumeQuery
{
    public class RecordModel
    {
        public string TypeID { get; set; }

        public string Type { get; set; }

        public string PartDescription { get; set; }

        public string Date { get; set; }

        public string VHNO { get; set; }

        public string Subject { get; set; }
    }
}
