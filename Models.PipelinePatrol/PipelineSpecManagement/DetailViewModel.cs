using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.PipelinePatrol.PipelineSpecManagement
{
    public class DetailViewModel
    {
        public Define.EnumOrganizationPermission Permission { get; set; }

        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "SpecType", ResourceType = typeof(Resources.Resource))]
        public string Type { get; set; }
        
        [Display(Name = "PipelineSpecDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        public List<string> OptionDescriptionList { get; set; }

        [Display(Name = "PipelineSpecOption", ResourceType = typeof(Resources.Resource))]
        public string Options
        {
            get
            {
                if (OptionDescriptionList != null && OptionDescriptionList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var option in OptionDescriptionList)
                    {
                        sb.Append(option);

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

            OptionDescriptionList = new List<string>();
        }
    }
}
