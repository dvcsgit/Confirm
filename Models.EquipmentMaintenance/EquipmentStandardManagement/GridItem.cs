using Utility;
namespace Models.EquipmentMaintenance.EquipmentStandardManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationDescription { get; set; }

        public string MaintenanceOrganization { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public GridItem()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
