using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.MaterialManagement
{
    public class DetailViewModel
    {
        public Define.EnumOrganizationPermission Permission { get; set; }

        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string Extension { get; set; }

        public string Photo {
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

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationDescription { get; set; }

        [Display(Name = "MaterialType", ResourceType = typeof(Resources.Resource))]
        public string EquipmentType { get; set; }

        [Display(Name = "MaterialID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "MaterialName", ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = "QTY", ResourceType = typeof(Resources.Resource))]
        public int Quantity { get; set; }

        public List<SpecModel> SpecList { get; set; }

        public List<EquipmentModel> EquipmentList { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;

            SpecList = new List<SpecModel>();

            EquipmentList=new List<EquipmentModel>();
        }
    }
}
