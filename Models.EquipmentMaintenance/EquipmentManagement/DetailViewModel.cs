using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.EquipmentManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "MaintenanceOrganization", ResourceType = typeof(Resources.Resource))]
        public string MaintenanceOrganizationFullDescription { get; set; }

        [Display(Name = "EquipmentID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "EquipmentName", ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        public string Extension { get; set; }

        public string Photo
        {
            get
            {
                if (!string.IsNullOrEmpty(Extension))
                {
                    return string.Format("{0}.{1}", UniqueID, Extension);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public List<SpecModel> SpecList { get; set; }

        public List<MaterialModel> MaterialList { get; set; }

        public List<PartModel> PartList { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
            SpecList = new List<SpecModel>();
            MaterialList = new List<MaterialModel>();
            PartList = new List<PartModel>();
        }
    }
}
