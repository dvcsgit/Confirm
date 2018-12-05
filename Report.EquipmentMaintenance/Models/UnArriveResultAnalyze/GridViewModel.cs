using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.UnArriveResultAnalyze
{
    public class GridViewModel
    {
        public QueryParameters Parameters { get; set; }

        public List<GridItem> ItemList { get; set; }

        public GridViewModel()
        {
            Parameters = new QueryParameters();
            ItemList = new List<GridItem>();
        }
    }
}
