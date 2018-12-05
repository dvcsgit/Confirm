using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.FlowManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "FlowDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        public List<VerifyOrganizationModel> VerifyOrganizationList { get; set; }

        public List<string> FormDescriptionList { get; set; }

        [Display(Name = "FormType", ResourceType = typeof(Resources.Resource))]
        public string Forms
        {
            get
            {
                if (FormDescriptionList != null && FormDescriptionList.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();

                    foreach (var formDescription in FormDescriptionList)
                    {
                        sb.Append(formDescription);

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
            VerifyOrganizationList = new List<VerifyOrganizationModel>();
            FormDescriptionList = new List<string>();
        }
    }
}
