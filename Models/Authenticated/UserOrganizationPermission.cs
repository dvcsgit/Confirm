using Utility;

namespace Models.Authenticated
{
    public class UserOrganizationPermission
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }
    }
}
