using System.Collections.Generic;

namespace Models.EquipmentMaintenance.RepairFormSubjectManagement
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
