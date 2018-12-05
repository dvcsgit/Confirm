using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.EquipmentDetail
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
        /// 材料的詳細資料
        /// </summary>
        public List<EquipmentDetailGridViewModel> EquipmentDetailList { get; set; }

        public GridItem()
        {
            EquipmentDetailList = new List<EquipmentDetailGridViewModel>();
        }


    }
}
