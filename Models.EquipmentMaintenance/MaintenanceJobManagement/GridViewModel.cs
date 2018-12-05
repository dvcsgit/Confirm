using System.Collections.Generic;

namespace Models.EquipmentMaintenance.MaintenanceJobManagement
{
    public class GridViewModel
    {
        public string OrganizationUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string FullOrganizationDescription { get; set; }

        public List<GridItem> ItemList { get; set; }

        public GridViewModel()
        {
            ItemList = new List<GridItem>();
        }
    }
}
