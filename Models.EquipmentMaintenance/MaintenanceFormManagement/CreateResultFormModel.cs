using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class CreateResultFormModel
    {
        public string Remark { get; set; }

        public List<StandardModel> StandardList { get; set; }

        public CreateResultFormModel()
        {
            StandardList = new List<StandardModel>();
        }
    }
}
