using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.RepairFormTypeManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "OrganizationDescription", ResourceType = typeof(Resources.Resource))]
        public string AncestorOrganizationDescription { get; set; }

        [Display(Name = "RepairFormTypeDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [DisplayName("納入MTTR計算")]
        public bool MTTR { get; set; }

        public bool CanDelete { get; set; }

        public List<SubjectModel> SubjectList { get; set; }

        public List<ColumnModel> ColumnList { get; set; }

        public DetailViewModel()
        {
            SubjectList = new List<SubjectModel>();
            ColumnList = new List<ColumnModel>();
        }
    }
}
