﻿using System.Collections.Generic;

namespace Models.PipelinePatrol.ConstructionFirmManagement
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
