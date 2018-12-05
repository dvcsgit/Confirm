using System.Collections.Generic;

namespace Models.ASE.QS.CheckListManagement
{
    public class GridViewModel
    {
        public List<FactoryModel> FactoryList { get; set; }

        public QueryParameters Parameters { get; set; }

        public List<GridItem> ItemList { get; set; }

        public GridViewModel()
        {
            Parameters = new QueryParameters();
            FactoryList = new List<FactoryModel>();
            ItemList = new List<GridItem>();
        }
    }
}
