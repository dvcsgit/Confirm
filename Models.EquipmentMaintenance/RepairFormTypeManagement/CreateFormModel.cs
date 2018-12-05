using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.RepairFormTypeManagement
{
    public class CreateFormModel
    {
        public string AncestorOrganizationUniqueID { get; set; }

        [Display(Name = "OrganizationDescription", ResourceType = typeof(Resources.Resource))]
        public string AncestorOrganizationDescription { get; set; }

        public FormInput FormInput { get; set; }

        public List<SubjectModel> SubjectList { get; set; }

        public List<ColumnModel> ColumnList { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
            SubjectList = new List<SubjectModel>();
            ColumnList = new List<ColumnModel>();
        }
    }
}
