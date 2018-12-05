using System.Collections.Generic;

namespace Models.EquipmentMaintenance.RepairFormColumnManagement
{
    public class GridViewModel
    {
        public string AncestorOrganizationUniqueID { get; set; }

        public string AncestorOrganizationDescription { get; set; }

        public List<GridItem> ItemList { get; set; }

        public GridViewModel()
        {
            ItemList = new List<GridItem>();
        }
    }
}
