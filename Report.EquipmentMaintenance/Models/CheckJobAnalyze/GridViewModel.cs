using System.Collections.Generic;

namespace Report.EquipmentMaintenance.Models.CheckJobAnalyze
{
    public class GridViewModel
    {
        public QueryParameters Parameters { get; set; }

        public List<GridItem> ItemList { get; set; }

        public List<string> CheckTime { get; set; }

        public GridViewModel()
        {
            Parameters = new QueryParameters();
            ItemList = new List<GridItem>();
        }
    }
}