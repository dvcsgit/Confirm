using Utility;
namespace Models.GuardPatrol.CheckItemManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationDescription { get; set; }

        public string CheckType { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public string Unit { get; set; }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(Unit))
                {
                    return string.Format("{0}({1})", Description, Unit);
                }
                else
                {
                    return Description;
                }
            }
        }

        public GridItem()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
