using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.TankPatrol.IslandManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [DisplayName("裝/卸料站代號")]
        public string StationID { get; set; }

        [DisplayName("裝/卸料站描述")]
        public string StationDescription { get; set; }

        [DisplayName("灌島代號")]
        public string IslandID { get; set; }

        [DisplayName("灌島描述")]
        public string IslandDescription { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
