using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.PFG.Models.DailyReport
{
    public class ReportModel
    {
        public string RouteUniqueID { get; set; }

        public string FileName
        {
            get
            {
                return string.Format("{0}熔爐區巡檢記錄表(主管每日表單)_{1}", OrganizationDescription, CheckDate);
            }
        }

        public string CheckDate { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public DateTime PrintTime { get; set; }

        public string CheckDateString
        {
            get
            {
                return DateTimeHelper.DateString2DateStringWithSeparator(CheckDate);
            }
        }

        public string PrintDateTimeString
        {
            get
            {
                return DateTimeHelper.DateTime2DateTimeStringWithSeperator(PrintTime);
            }
        }

        public List<ControlPointModel> ControlPointList { get; set; }

        public ReportModel()
        {
            ControlPointList = new List<ControlPointModel>();
        }
    }
}
