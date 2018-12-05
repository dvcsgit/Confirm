using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.TankPatrol.AbnormalReasonManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "AbnormalType", ResourceType = typeof(Resources.Resource))]
        public string AbnormalType { get; set; }

        [Display(Name = "AbnormalReasonID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "AbnormalReasonDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        public List<string> HandlingMethodDescriptionList { get; set; }

        [Display(Name = "HandlingMethod", ResourceType = typeof(Resources.Resource))]
        public string HandlingMethods
        {
            get
            {
                if (this.HandlingMethodDescriptionList != null && this.HandlingMethodDescriptionList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var handlingMethod in this.HandlingMethodDescriptionList)
                    {
                        sb.Append(handlingMethod);

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
            HandlingMethodDescriptionList = new List<string>();
        }
    }
}
