using System.Collections.Generic;
using Utility;

namespace Models.PipelinePatrol.CheckPointManagement
{
    public class GridViewModel
    {
        public string FullOrganizationDescription { get; set; }

        public List<GridItem> ItemList { get; set; }

        public GridViewModel()
        {
            ItemList = new List<GridItem>();
        }
    }
}
