using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.PipelinePatrol.AbnormalReasonManagement
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

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
