using System.Collections.Generic;

namespace Models.PipelinePatrol.MobileRelease
{
    public class GridViewModel
    {
        public List<GridItem> ItemList { get; set; }

        public GridViewModel()
        {
            ItemList = new List<GridItem>();
        }
    }
}
