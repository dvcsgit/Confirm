using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.EquipmentCost
{
  public  class GridItem
    {
        /// <summary>
        /// 設備編號
        /// </summary>
        public string EquipmentID { get; set; }

        /// <summary>
        /// 設備名稱
        /// </summary>
        public string EquipmentName { get; set; }

        /// <summary>
        /// 保養單位
        /// </summary>
        public string EquipmentMaintenanceOrganizationUniqueID { get; set; }

        /// <summary>
        /// 保管單位
        /// </summary>
        public string EquipmentOrganizationUniqueID { get; set; }

        /// <summary>
        /// 代理商
        /// </summary>
        public string EquipmentSpecValueAgent { get; set; }

        /// <summary>
        /// 出廠編號
        /// </summary>
        public string EquipmentSpecValueFactoryNumber { get; set; }

        /// <summary>
        /// 型號
        /// </summary>
        public string EquipmentSpecValueModel { get; set; }


        /// <summary>
        /// 保養工時合計
        /// </summary>
        public string MaintenanceCostTotal { get; set; }

        /// <summary>
        /// 修復工時合計
        /// </summary>
        public string RepairCostTotal { get; set; }


        public string MaterialSumPrice 
        { 
            get
            {
                if (EquipmentCostList != null && EquipmentCostList.Count != 0)
                {
                    return EquipmentCostList.Sum(x => x.MaterialTotalPrice).ToString();
                }
                else
                {
                    return "0";
                }
            } 
        }

        public string TotalPrice 
        {
            get 
            {
                if (EquipmentCostList != null && EquipmentCostList.Count != 0)
                {
                    return EquipmentCostList.Sum(x => x.MaterialTotalPrice).ToString();
                }
                else
                {
                    return "0";
                }
                
            }
        }


        /// <summary>
        /// 材料的詳細資料
        /// </summary>
        public List<EquipmentCostGridViewModel> EquipmentCostList { get; set; }

        public GridItem()
        {
            EquipmentCostList = new List<EquipmentCostGridViewModel>();
        }

    }
}
