using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.RouteManagement
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

        public List<Job> JobList { get; set; }

        public List<ManagerModel> ManagerList { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            JobList = new List<Job>();
            ManagerList = new List<ManagerModel>();
        }
    }
}
