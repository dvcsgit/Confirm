using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.Models.EReport
{
    public class ControlPointModel
    {
        public string ControlPointDescription { get; set; }

        public List<EquipmentModel> EquipmentList { get; set; }

        public ControlPointModel()
        {
            EquipmentList = new List<EquipmentModel>();
        }
    }
}
