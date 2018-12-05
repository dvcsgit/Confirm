using System.Collections.Generic;

namespace Models.PipelinePatrol.FileManagement
{
    public class GridViewModel
    {
        public QueryParameters Parameters { get; set; }

        public string PathDescription { get; set; }

        public string FullPathDescription { get; set; }

        public List<GridItem> ItemList { get; set; }

        public GridViewModel()
        {
            Parameters = new QueryParameters();
            ItemList = new List<GridItem>();
        }
    }
}
