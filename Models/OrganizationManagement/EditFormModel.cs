using Models.Shared;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models.OrganizationManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string AncestorOrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        //public string ManagerName { get; set; }

        //public string Manager
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(ManagerName))
        //        {
        //            return string.Format("{0}/{1}", FormInput.ManagerUserID, ManagerName);
        //        }
        //        else
        //        {
        //            return FormInput.ManagerUserID;
        //        }
        //    }
        //}

        //public List<UserModel> UserList { get; set; }

        public List<string> ManagerList { get; set; }

        public string Managers
        {
            get
            {
                if (ManagerList != null && ManagerList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var manager in ManagerList)
                    {
                        sb.Append(manager);
                        sb.Append(",");
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

        public FormInput FormInput { get; set; }

        public List<EditableOrganizationModel> EditableOrganizationList { get; set; }

        public List<QueryableOrganizationModel> QueryableOrganizationList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            //UserList = new List<UserModel>();
            ManagerList = new List<string>();
            EditableOrganizationList = new List<EditableOrganizationModel>();
            QueryableOrganizationList = new List<QueryableOrganizationModel>();
        }
    }
}
