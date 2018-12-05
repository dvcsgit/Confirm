using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.PipelinePatrol.RouteManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "RouteID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "RouteName", ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

         [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }

         public List<PipelineModel> PipelineList { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;

            PipelineList = new List<PipelineModel>();
        }
    }
}
