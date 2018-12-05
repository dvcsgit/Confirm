using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.EquipmentDetail
{
  public  class GridViewModel
    {
        public QueryParameters Parameters { get; set; }

        public GridItem GridItem { get; set; }

        public GridViewModel()
        {
            Parameters = new QueryParameters();
            GridItem = new GridItem();
        }
    }
}
