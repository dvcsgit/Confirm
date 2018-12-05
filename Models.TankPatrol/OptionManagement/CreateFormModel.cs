using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.TankPatrol.OptionManagement
{
    public class CreateFormModel
    {
        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<SelectListItem> TypeSelectItemList
        {
            get
            {
                return new List<SelectListItem>() 
                { 
                    Define.DefaultSelectListItem(Resources.Resource.SelectOne),
                    new SelectListItem() { Text = "車牌號碼", Value = "C" },
                    new SelectListItem() { Text = "司機", Value = "D" },
                    new SelectListItem() { Text = "貨主", Value = "O" },
                    new SelectListItem() { Text = "乘載物質", Value = "M" }
                };
            }
        }

        public CreateFormModel()
        {
            FormInput = new FormInput();
        }
    }
}
