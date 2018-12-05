using Utility;
namespace Models.TankPatrol.OptionManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationDescription { get; set; }

        public string Type { get; set; }
        
        public string TypeDisplay
        {
            get
            {
                if (Type == "C")
                {
                    return "車牌號碼";
                }
                else if (Type == "D")
                {
                    return "司機";
                }
                else if (Type == "O")
                {
                    return "貨主";
                }
                else if (Type == "M")
                {
                    return "乘載物質";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Description { get; set; }

        public GridItem()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
