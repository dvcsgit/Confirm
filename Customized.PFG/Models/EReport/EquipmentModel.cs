using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.Models.EReport
{
    public class EquipmentModel
    {
        public string EquipmentName { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public EquipmentModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
