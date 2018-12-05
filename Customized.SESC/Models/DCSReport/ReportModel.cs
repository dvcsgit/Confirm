using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.SESC.Models.DCSReport
{
    public class ReportModel
    {
        public string Date { get; set; }

        public string RouteName { get; set; }

        public List<ItemModel> ItemList { get; set; }

        public ReportModel()
        {
            ItemList = new List<ItemModel>();
        }
    }
}
