using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.EquipmentFixCost
{
    public class GridItem
    {
        [DisplayName("組織")]
        public string OrganizationDescription { get; set; }

        /// <summary>
        /// 設備編號
        /// </summary>
        [DisplayName("設備代號")]
        public string EquipmentID { get; set; }

        /// <summary>
        /// 設備名稱
        /// </summary>
        [DisplayName("設備名稱")]
        public string EquipmentName { get; set; }

        /// <summary>
        /// 保養單位
        /// </summary>
        [DisplayName("保養單位")]
        public string MaintenanceOrganizationDescription { get; set; }

        public List<RepairFormModel> RepairFormList { get; set; }

        [DisplayName("異常次數")]
        public int AbnormalCount
        {
            get
            {
                return RepairFormList.Count;
            }
        }

        [DisplayName("MTBF")]
        public double MTBF
        {
            get
            {
                return Math.Round(Duration / AbnormalCount, 1);
            }
        }

        [DisplayName("MTTR")]
        public double MTTR
        {
            get
            {
                return Math.Round(RepairFormList.Sum(x => x.Duration) / AbnormalCount, 1);
            }
        }

        public double Duration { get; set; }

        public GridItem()
        {
            RepairFormList = new List<RepairFormModel>();
        }
    }
}
