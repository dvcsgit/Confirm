using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.RepairFormColumnManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "OrganizationDescription", ResourceType = typeof(Resources.Resource))]
        public string AncestorOrganizationDescription { get; set; }

        public FormInput FormInput { get; set; }
        
        public List<OptionModel> OptionList { get; set; }
        
        public EditFormModel()
        {
            FormInput = new FormInput();
            OptionList = new List<OptionModel>();
        }
    }
}
