using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;
using Models.PipelinePatrol.Shared;

namespace Models.PipelinePatrol.PipePointManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "PipePointType", ResourceType = typeof(Resources.Resource))]
        public string PipePointType { get; set; }

        [Display(Name = "PipePointID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "PipePointName", ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }

        public Location Location { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            Location = new Location();
        }
    }
}
