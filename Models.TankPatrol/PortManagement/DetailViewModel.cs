using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.TankPatrol.PortManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [DisplayName("裝/卸料站描述")]
        public string StationDescription { get; set; }

        [DisplayName("灌島描述")]
        public string IslandDescription { get; set; }

        [DisplayName("灌口代號")]
        public string ID { get; set; }

        [DisplayName("灌口描述")]
        public string Description { get; set; }

        [DisplayName("TagID")]
        public string TagID { get; set; }

        public int? LB2LPTimeSpan { get; set; }

        public int? LP2LATimeSpan { get; set; }

        public int? LA2LDTimeSpan { get; set; }

        public int? UB2UPTimeSpan { get; set; }

        public int? UP2UATimeSpan { get; set; }

        public int? UA2UDTimeSpan { get; set; }

        public List<CheckItemModel> LBCheckItemList { get; set; }
        public List<CheckItemModel> LPCheckItemList { get; set; }
        public List<CheckItemModel> LACheckItemList { get; set; }
        public List<CheckItemModel> LDCheckItemList { get; set; }
        public List<CheckItemModel> UBCheckItemList { get; set; }
        public List<CheckItemModel> UPCheckItemList { get; set; }
        public List<CheckItemModel> UACheckItemList { get; set; }
        public List<CheckItemModel> UDCheckItemList { get; set; }

        public DetailViewModel()
        {
            LBCheckItemList = new List<CheckItemModel>();
            LPCheckItemList = new List<CheckItemModel>();
            LACheckItemList = new List<CheckItemModel>();
            LDCheckItemList = new List<CheckItemModel>();
            UBCheckItemList = new List<CheckItemModel>();
            UPCheckItemList = new List<CheckItemModel>();
            UACheckItemList = new List<CheckItemModel>();
            UDCheckItemList = new List<CheckItemModel>();
        }
    }
}
