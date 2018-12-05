using System.Collections.Generic;
using System.Text;
using Utility;

namespace Models.UserManagement
{
    public class GridItem
    {
        public string OrganizationDescription { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string Title { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public bool IsMobileUser { get; set; }

        public List<string> AuthGroupList { get; set; }

        public string AuthGroups
        {
            get
            {
                if (AuthGroupList != null && AuthGroupList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var authGroup in AuthGroupList)
                    {
                        sb.Append(authGroup);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public GridItem()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
