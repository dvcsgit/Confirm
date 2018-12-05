using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.TruckPatrol.TruckManagement
{
    public class GridViewModel
    {
        public Define.EnumOrganizationPermission Permission { get; set; }

        public List<GridItem> ItemList { get; set; }

        public GridViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            ItemList = new List<GridItem>();
        }
    }
}
