using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.EquipmentCheckItemManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "EquipmentID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "EquipmentName", ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = "IsFeelItemDefaultNormal", ResourceType = typeof(Resources.Resource))]
        public bool IsFeelItemDefaultNormal { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public List<PartModel> PartList { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            CheckItemList = new List<CheckItemModel>();
            PartList = new List<PartModel>();
        }
    }
}
