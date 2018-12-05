using Utility;

namespace Models.EquipmentMaintenance.MaterialManagement
{
    public class GridItem
    {
        public Define.EnumOrganizationPermission Permission { get; set; }

        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string EquipmentType { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public GridItem()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
