using System.Web.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.RepairFormSubjectManagement
{
    public class CreateFormModel
    {
        public string AncestorOrganizationUniqueID { get; set; }

        [Display(Name = "OrganizationDescription", ResourceType = typeof(Resources.Resource))]
        public string AncestorOrganizationDescription { get; set; }

        public FormInput FormInput { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
        }
    }
}
