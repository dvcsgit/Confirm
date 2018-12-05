using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.EquipmentFixHour
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

        public int TotalCount { get; set; }

        public string MTTR
        {
            get
            {
                if (TotalCount != 0)
                    //if (TotalCount != null && TotalCount != 0)
                {
                    return (TotalSumHour / TotalCount).ToString("0.00");
                }
                else
                {
                    return "0";
                }
            }
        }

        public double TotalSumHour
        {
            get
            {
                if (RFormWorkingHourModelList != null && RFormWorkingHourModelList.Count != 0)
                {
                    return Convert.ToDouble(RFormWorkingHourModelList.Sum(x => (x.Hour)));
                }
                else
                {
                    return 0.00;
                }
            }

        }

        public List<RFormWorkingHourModel> RFormWorkingHourModelList { get; set; }

        public GridItem()
        {
            RFormWorkingHourModelList = new List<RFormWorkingHourModel>();
        }
    }
}
