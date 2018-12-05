using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.EquipmentMaintenance.RepairFormManagement
{
    public class ExportModel
    {
        /// <summary>
        /// 本單編號
        /// </summary>
        public string RformVHNO { get; set; }

        /// <summary>
        /// 保養單位
        /// </summary>
        public string MaintenanceOrganizationName { get; set; }

        /// <summary>
        /// 修復單類型
        /// </summary>
        public string RFormTypeName { get; set; }

        /// <summary>
        /// 設備ID
        /// </summary>
        public string EquipmentUniqueID { get; set; }

        /// <summary>
        /// 設備編號
        /// </summary>
        public string EquipmentID { get; set; }

        /// <summary>
        /// 設備名稱
        /// </summary>
        public string EquipmentUniqueName { get; set; }

        /// <summary>
        /// 部位編號
        /// </summary>
        public string PartUniqueID { get; set; }

        /// <summary>
        /// 部位名稱
        /// </summary>
        public string PartUniqueName { get; set; }

        /// <summary>
        /// 預計保養日
        /// </summary>
        public string EstBeginDate { get; set; }

        /// <summary>
        /// 實際保養日
        /// </summary>
        public string BeginDate { get; set; }

        /// <summary>
        /// 驗收人員
        /// </summary>
        public string AcceptanceUserID { get; set; }

        /// <summary>
        /// 驗收時間
        /// </summary>
        public string AcceptanceDate { get; set; }

        /// <summary>
        /// 填單人員
        /// </summary>
        public string CreateUserName { get; set; }

        ///// <summary>
        ///// 填單時間
        ///// </summary>
        //public string CreateTime { get; set; }待定

        /// <summary>
        /// 修復主旨
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 修復內容
        /// </summary>
        public string Description { get; set; }    
    }
}
