using Utility;
namespace Models.EquipmentMaintenance.MaterialSpecManagement
{
    public class GridItem
    {
        public Define.EnumOrganizationPermission Permission { get; set; }

        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string MaterialType { get; set; }
        
        public string Description { get; set; }

        public GridItem()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
