using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models.EquipmentMaintenance.DailyReport
{
    public class ReportModel
    {
        public string Date { get; set; }

        public string OrganizationDescription { get; set; }

        public string RouteName { get; set; }

        public List<JobModel> JobList { get; set; }

        public List<CheckItemModel> ItemList { get; set; }

        public ReportModel()
        {
            JobList = new List<JobModel>();
            ItemList = new List<CheckItemModel>();
        }
    }
}
