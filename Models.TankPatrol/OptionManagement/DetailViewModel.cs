using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.TankPatrol.OptionManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public Define.EnumOrganizationPermission Permission { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public string Type { get; set; }

        [DisplayName("選項類別")]
        public string TypeDisplay
        {
            get
            {
                if (Type == "C")
                {
                    return "車牌號碼";
                }
                else if (Type == "D")
                {
                    return "司機";
                }
                else if (Type == "O")
                {
                    return "貨主";
                }
                else if (Type == "M")
                {
                    return "乘載物質";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [DisplayName("選項描述")]
        public string Description { get; set; }

        public DetailViewModel()
        {
            Permission = Define.EnumOrganizationPermission.None;
        }
    }
}
