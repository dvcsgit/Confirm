using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.HandlingMethodManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "HandlingMethodType", ResourceType = typeof(Resources.Resource))]
        public string HandlingMethodType { get; set; }

        [Display(Name = "HandlingMethodID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "HandlingMethodDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
