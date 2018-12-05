using Models.Shared;
using System.Collections.Generic;
using Utility;

namespace Models.EquipmentMaintenance.EquipmentCheckItemManagement
{
    public class GridViewModel
    {
        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string FullOrganizationDescription { get; set; }

        public List<GridItem> ItemList { get; set; }

        public List<MoveToTarget> MoveToTargetList { get; set; }

        public GridViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            ItemList = new List<GridItem>();
            MoveToTargetList=new List<MoveToTarget>();
        }
    }
}
