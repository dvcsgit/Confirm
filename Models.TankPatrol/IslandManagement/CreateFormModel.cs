using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Models.TankPatrol.IslandManagement
{
    public class CreateFormModel
    {
        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public List<SelectListItem> StationSelectItemList { get; set; }   

        public FormInput FormInput { get; set; }

        public CreateFormModel()
        {
            StationSelectItemList = new List<SelectListItem>();
            FormInput = new FormInput();
        }
    }
}
