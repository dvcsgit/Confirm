using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.PFG.CN.Models.DailyReport
{
    public class ReportModel
    {
        public string RouteUniqueID { get; set; }

        public string FileName
        {
            get
            {
                return string.Format("變電站巡檢抄錶記錄_{0}",CheckDate);
            }
        }

        public string CheckDate { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public DateTime ReportTime { get; set; }

        public string ReportDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(ReportTime);
            }
        }

        public string ReportTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2TimeStringWithSeperator(ReportTime);
            }
        }

       
    }
}
