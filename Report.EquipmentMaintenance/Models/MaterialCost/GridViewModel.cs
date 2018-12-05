using System.Collections.Generic;

namespace Report.EquipmentMaintenance.Models.MaterialCost
{
    public class GridViewModel
    {
        public QueryParameters Parameters { get; set; }

        public List<MaterialReportModel> ItemList { get; set; }

        public GridViewModel()
        {
            Parameters = new QueryParameters();
            ItemList = new List<MaterialReportModel>();
        }
    }
}
