using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.EquipmentCost
{
   public class EquipmentCostGridViewModel
    {
        /// <summary>
        /// 材料編號
        /// </summary>
        public string MaterialID { get; set; }

        /// <summary>
        /// 材料名稱
        /// </summary>
        public string MaterialName { get; set; }

        /// <summary>
        /// 材料規格
        /// </summary>
        public string MaterialSpecValueMaterialStandard { get; set; }

        /// <summary>
        /// 材料單價
        /// </summary>
        public string MaterialPrice { get; set; }

        /// <summary>
        /// 更換數量
        /// </summary>
        public string MaterialChageNumber { get; set; }

        /// <summary>
        /// 材料總價
        /// </summary>
        public double MaterialTotalPrice { get; set; }
    }
}
