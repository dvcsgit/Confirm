using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PagedList;

namespace Models.ASE.QA.EquipmentManagement
{
    public class GridViewModel
    {
        public QueryParameters Parameters { get; set; }

        public IPagedList<GridItem> ItemList { get; set; }

        public GridViewModel()
        {
            Parameters = new QueryParameters();
        }
    }
}
