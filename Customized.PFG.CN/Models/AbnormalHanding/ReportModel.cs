using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.CN.Models.AbnormalHanding
{
    public class ReportModel
    {
        public string RouteUniqueID { get; set; }

        public string FileName
        {
            get
            {
                return string.Format("預防巡檢異常匯總表");
            }
        }
        public QueryParameters Parameters { get; set; }
        public string CheckDate { get; set; }
        public string CheckTime { get; set; }
        public string CheckUser { get; set; }
        public string RouteName { get; set; }
        public string OrganizationUniqueID { get; set; }
        public string OrganizationDescription { get; set; }
        public List<CheckItemModel> ItemList { get; set; }

        public ReportModel()
        {
            Parameters = new QueryParameters();
            ItemList = new List<CheckItemModel>();
        }
    }
}
