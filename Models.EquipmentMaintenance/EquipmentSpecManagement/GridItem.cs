using Utility;
namespace Models.EquipmentMaintenance.EquipmentSpecManagement
{
    public class GridItem
    {
        public Define.EnumOrganizationPermission Permission { get; set; }

        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string EquipmentType { get; set; }
        
        public string Description { get; set; }

        public GridItem()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
