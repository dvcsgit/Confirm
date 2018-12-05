using System.Collections.Generic;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
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
