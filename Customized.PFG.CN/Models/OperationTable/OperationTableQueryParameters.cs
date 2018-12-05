using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Utility;


namespace Customized.PFG.CN.Models.OperationTable
{
    public class OperationTableQueryParameters
    {
        /// <summary>
        /// 组织
        /// </summary>
        public string OrganizationUniqueID { get; set; }
            
        /// <summary>
        /// 组织描述
        /// </summary>
        public string OrganizationDescription { get; set; }

        public string OrganizationID { get; set; }

        [Display(Name = "BeginDate", ResourceType = typeof(Resources.Resource))]
        public string BeginDateString { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string BeginDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(BeginDateString);
            }
        }

        [Display(Name = "EndDate", ResourceType = typeof(Resources.Resource))]
        public string EndDateString { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndDate
        {
            get
            {
                return DateTimeHelper.DateStringWithSeperator2DateString(EndDateString);
            }
        }
        /// <summary>
        /// 路线ID
        /// </summary>
        public string RouteUniqueID { get; set; }

        /// <summary>
        /// 巡检点代号
        /// </summary>
        /// 

        public string ControlPointUniqueID { get; set; }

        /// <summary>
        /// 设备代号
        /// </summary>
        /// 

        public string EquipmentUniqueID { get; set; }

    }
}

