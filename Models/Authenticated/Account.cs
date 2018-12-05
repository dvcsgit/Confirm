using System.Linq;
using System.Collections.Generic;
using Utility;
using Models.Shared;

namespace Models.Authenticated
{
    public class Account
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string UserRootOrganizationUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string Photo { get; set; }

        public List<string> UserAuthGroupList { get; set; }

        public List<MenuItem> MenuItemList { get; set; }

        public List<UserWebPermissionFunction> WebPermissionFunctionList { get; set; }

        public List<UserOrganizationPermission> UserOrganizationPermissionList { get; set; }

        public List<string> VisibleOrganizationUniqueIDList
        {
            get
            {
                return UserOrganizationPermissionList.Select(x => x.UniqueID).ToList();
            }
        }

        public List<string> QueryableOrganizationUniqueIDList
        {
            get
            {
                return UserOrganizationPermissionList.Where(x => x.Permission == Define.EnumOrganizationPermission.Queryable || x.Permission == Define.EnumOrganizationPermission.Editable).Select(x => x.UniqueID).ToList();
            }
        }

        public List<string> EditableOrganizationUniqueIDList
        {
            get
            {
                return UserOrganizationPermissionList.Where(x => x.Permission == Define.EnumOrganizationPermission.Editable).Select(x => x.UniqueID).ToList();
            }
        }

        public Define.EnumOrganizationPermission OrganizationPermission(string OrganizationUniqueID)
        {
            if (EditableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
            {
                return Define.EnumOrganizationPermission.Editable;
            }
            else if (QueryableOrganizationUniqueIDList.Contains(OrganizationUniqueID))
            {
                return Define.EnumOrganizationPermission.Queryable;
            }
            else if (VisibleOrganizationUniqueIDList.Contains(OrganizationUniqueID))
            {
                return Define.EnumOrganizationPermission.Visible;
            }
            else
            {
                return Define.EnumOrganizationPermission.None;
            }
        }

        public Account()
        {
            UserAuthGroupList = new List<string>();
            MenuItemList = new List<MenuItem>();
            WebPermissionFunctionList = new List<UserWebPermissionFunction>();
            UserOrganizationPermissionList = new List<UserOrganizationPermission>();
        }
    }
}
