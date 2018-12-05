using System.Collections.Generic;
using Utility;

namespace Models.EquipmentMaintenance.RepairFormManagement
{
    public class GridViewModel
    {
        public string OrganizationUniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationDescription { get; set; }

        public string FullOrganizationDescription { get; set; }

        public List<GridItem> ItemList { get; set; }

        public GridViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            ItemList = new List<GridItem>();
        }
    }
}
