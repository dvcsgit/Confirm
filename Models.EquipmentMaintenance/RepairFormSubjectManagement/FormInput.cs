using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Utility;

namespace Models.EquipmentMaintenance.RepairFormSubjectManagement
{
    public class FormInput
    {
        [Display(Name = "RepairFormSubjectID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "RepairFormSubjectIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "RepairFormSubjectDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "RepairFormSubjectDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
    }
}
