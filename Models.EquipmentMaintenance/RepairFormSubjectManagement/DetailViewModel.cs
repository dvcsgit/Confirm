using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.RepairFormSubjectManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        [Display(Name = "OrganizationDescription", ResourceType = typeof(Resources.Resource))]
        public string AncestorOrganizationDescription { get; set; }

        [Display(Name = "RepairFormSubjectID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "RepairFormSubjectDescription", ResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
    }
}
