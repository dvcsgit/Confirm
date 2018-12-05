using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.EquipmentFixHour
{
  public   class RFormWorkingHourModel
    {
        public string RFormTypeUniqueID { get; set; }

        public double? Hour { get; set; }

        public string Count { get; set; }
    }
}
