using System.Collections.Generic;
using Utility;

namespace Models.ASE.QA.MSACharacteristicManagement
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
