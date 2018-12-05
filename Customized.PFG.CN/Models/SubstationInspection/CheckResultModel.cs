using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.CN.Models.SubstationInspection
{
    public class CheckResultModel
    {
        public string EquipmentUniqueID { get; set; }
        public string EquipmentName { get; set; }
        public double Value { get; set; }
        public string CheckDate { get; set; }
  

    }
}
