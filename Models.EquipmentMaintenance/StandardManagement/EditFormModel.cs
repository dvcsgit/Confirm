using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Models.EquipmentMaintenance.StandardManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<SelectListItem> MaintenanceTypeSelectItemList { get; set; }

        public List<FeelOptionModel> FeelOptionList { get; set; }

        public List<AbnormalReasonModel> AbnormalReasonList { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
            MaintenanceTypeSelectItemList = new List<SelectListItem>();
            FeelOptionList = new List<FeelOptionModel>();
            AbnormalReasonList = new List<AbnormalReasonModel>();
        }
    }
}
