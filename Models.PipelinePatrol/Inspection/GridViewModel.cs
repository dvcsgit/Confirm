using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.PipelinePatrol.Inspection
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
