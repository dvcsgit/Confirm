using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.TankPatrol.PortManagement
{
    public class CreateFormModel
    {
        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public List<SelectListItem> StationSelectItemList { get; set; }

        public List<SelectListItem> IslandSelectItemList { get; set; }

        public List<IslandModel> IslandList { get; set; }

        public FormInput FormInput { get; set; }

        public List<CheckItemModel> LBCheckItemList { get; set; }
        public List<CheckItemModel> LPCheckItemList { get; set; }
        public List<CheckItemModel> LACheckItemList { get; set; }
        public List<CheckItemModel> LDCheckItemList { get; set; }
        public List<CheckItemModel> UBCheckItemList { get; set; }
        public List<CheckItemModel> UPCheckItemList { get; set; }
        public List<CheckItemModel> UACheckItemList { get; set; }
        public List<CheckItemModel> UDCheckItemList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            LBCheckItemList = new List<CheckItemModel>();
            LPCheckItemList = new List<CheckItemModel>();
            LACheckItemList = new List<CheckItemModel>();
            LDCheckItemList = new List<CheckItemModel>();
            UBCheckItemList = new List<CheckItemModel>();
            UPCheckItemList = new List<CheckItemModel>();
            UACheckItemList = new List<CheckItemModel>();
            UDCheckItemList = new List<CheckItemModel>();
            StationSelectItemList = new List<SelectListItem>();
            IslandSelectItemList = new List<SelectListItem>();
            IslandList = new List<IslandModel>();
        }
    }
}
