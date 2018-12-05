using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.JobManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "JobID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "JobDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        public List<RouteModel> RouteList { get; set; }

        public DetailViewModel()
        {
            RouteList = new List<RouteModel>();
        }
    }
}
