using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.ControlPointManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "ControlPointID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "ControlPointDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [Display(Name = "TagID", ResourceType = typeof(Resources.Resource))]
        public string TagID { get; set; }

        [Display(Name = "IsFeelItemDefaultNormal", ResourceType = typeof(Resources.Resource))]
        public bool IsFeelItemDefaultNormal { get; set; }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
