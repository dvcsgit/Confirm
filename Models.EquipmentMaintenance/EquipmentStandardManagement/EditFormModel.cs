using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.EquipmentMaintenance.EquipmentStandardManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        [Display(Name = "ParentOrganization", ResourceType = typeof(Resources.Resource))]
        public string ParentOrganizationFullDescription { get; set; }

        [Display(Name = "MaintenanceOrganization", ResourceType = typeof(Resources.Resource))]
        public string MaintenanceOrganizationFullDescription { get; set; }

        [Display(Name = "EquipmentID", ResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "EquipmentName", ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        public List<StandardModel> StandardList { get; set; }

        public List<PartModel> PartList { get; set; }
        
        public EditFormModel()
        {
            StandardList = new List<StandardModel>();
            PartList = new List<PartModel>();
        }
    }
}
