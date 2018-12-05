using System.Collections.Generic;

namespace Customized.SESC.Models.CheckReport
{
    public class ReportModel
    {
        public string Date { get; set; }

        public string RouteName { get; set; }

        public List<JobModel> JobList { get; set; }

        public List<ItemModel> ItemList { get; set; }

        public ReportModel()
        {
            JobList = new List<JobModel>();
            ItemList = new List<ItemModel>();
        }
    }
}
