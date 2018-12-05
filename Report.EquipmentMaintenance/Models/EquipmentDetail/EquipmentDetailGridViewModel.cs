using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.EquipmentDetail
{
  public   class EquipmentDetailGridViewModel
    {

        /// <summary>
        /// 部位名稱
        /// </summary>
        public string EquipmentPartDescription { get; set; }

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
        /// 數量
        /// </summary>
        public int EquipmentMaterialQTY { get; set; }

    }
}
