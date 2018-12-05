using DbEntity.MSSQL.EquipmentMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.PFG.Models.EReport
{
    public class ReportModel
    {
        public string RouteUniqueID { get; set; }

        public string FileName
        {
            get
            {
                return string.Format("電儀課巡檢紀錄表_{0}", CheckDate);
            }
        }

        public string CheckDate { get; set; }

        public string CheckUser { get; set; }

        public List<ControlPointModel> ControlPointList { get; set; }

        public ReportModel()
        {
            ControlPointList = new List<ControlPointModel>();
        }
    }
}
