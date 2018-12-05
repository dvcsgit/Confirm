using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.UserManagement
{
    public class DetailViewModel
    {
        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "UserID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "Title", ResourceType = typeof(Resources.Resource))]
        public string Title { get; set; }

        [Display(Name = "UserName", ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = "UID", ResourceType = typeof(Resources.Resource))]
        public string UID { get; set; }

        [Display(Name = "EMail", ResourceType = typeof(Resources.Resource))]
        public string EMail { get; set; }

        [Display(Name = "IsMobileUser", ResourceType = typeof(Resources.Resource))]
        public bool IsMobileUser { get; set; }

        public List<string> AuthGroupNameList { get; set; }

        [Display(Name = "AuthGroup", ResourceType = typeof(Resources.Resource))]
        public string AuthGroups
        {
            get
            {
                if (AuthGroupNameList != null && AuthGroupNameList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var authGroup in this.AuthGroupNameList)
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

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            AuthGroupNameList = new List<string>();
        }
    }
}
