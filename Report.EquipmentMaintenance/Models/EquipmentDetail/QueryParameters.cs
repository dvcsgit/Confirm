using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Report.EquipmentMaintenance.Models.EquipmentDetail
{
   public  class QueryParameters
    {
        /// <summary>
        /// 设备的UniqueID
        /// </summary>
        public string EquipmentUniqueID { get; set; }

        /// <summary>
        /// 下载Excel的类型
        /// </summary>
       public  Define.EnumExcelVersion ExcelVersion { get; set; }
    }
}
