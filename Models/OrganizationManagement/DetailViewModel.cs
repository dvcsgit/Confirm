using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Utility;

namespace Models.OrganizationManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "OrganizationID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "OrganizationDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        //public string ManagerUserID { get; set; }

        //public string ManagerName { get; set; }

        //[Display(Name = "Manager", ResourceType = typeof(Resources.Resource))]
        //public string Manager
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(ManagerName))
        //        {
        //            return string.Format("{0}/{1}", ManagerUserID, ManagerName);
        //        }
        //        else
        //        {
        //            return ManagerUserID;
        //        }
        //    }
        //}

        public List<Models.Shared.UserModel> ManagerList { get; set; }

        [Display(Name = "Manager", ResourceType = typeof(Resources.Resource))]
        public string Managers
        {
            get
            {
                if (ManagerList != null && ManagerList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var manager in ManagerList)
                    {
                        sb.Append(manager.User);
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

        public List<string> EditableOrganizationList { get; set; }

        public List<string> QueryableOrganizationList { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            ManagerList = new List<Shared.UserModel>();
            EditableOrganizationList = new List<string>();
            QueryableOrganizationList = new List<string>();
        }
    }
}
