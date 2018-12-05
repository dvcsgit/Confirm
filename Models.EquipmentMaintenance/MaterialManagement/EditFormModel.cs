using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Models.EquipmentMaintenance.MaterialManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

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

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<SelectListItem> EquipmentTypeSelectItemList { get; set; }

        public List<SpecModel> SpecList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            EquipmentTypeSelectItemList = new List<SelectListItem>();
            SpecList = new List<SpecModel>();
        }
    }
}
