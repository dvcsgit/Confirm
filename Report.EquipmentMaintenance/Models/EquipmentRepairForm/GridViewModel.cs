using System.Collections.Generic;
using Utility;

namespace Report.EquipmentMaintenance.Models.EquipmentRepairForm
{
    public class GridViewModel
    {
        public string OrganizationUniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationDescription { get; set; }

        public string FullOrganizationDescription { get; set; }



        public List<GridItem> ItemList { get; set; }

        public ExportViewModel Export { get; set; }

        public List<ExportItem> ExportList { get; set; }

        public GridViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            ItemList = new List<GridItem>();
            Export = new ExportViewModel();
            ExportList = new List<ExportItem>();
        }
    }
}
