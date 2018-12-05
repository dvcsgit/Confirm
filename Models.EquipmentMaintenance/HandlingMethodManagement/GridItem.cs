using Utility;
namespace Models.EquipmentMaintenance.HandlingMethodManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationDescription { get; set; }

        public string HandlingMethodType { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public GridItem()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
