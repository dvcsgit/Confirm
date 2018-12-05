using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report.EquipmentMaintenance.Models.EquipmentFixHour
{
   public class GridViewModel
    {
        public QueryParameters Parameters { get; set; }

        public List<GridItem> GridItem { get; set; }

        public List<RFormTypeModel> RFormTypeList { get; set; }

        public List<RFormTypeSumModel> RFormTypeSumModelList { get; set; }

        public GridViewModel()
        {
            Parameters = new QueryParameters();
            GridItem = new List<GridItem>();
            RFormTypeList = new List<RFormTypeModel>();
            RFormTypeSumModelList = new List<RFormTypeSumModel>();
        }
    }
}
