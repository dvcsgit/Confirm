using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.EquipmentMaintenance.RepairFormTypeManagement
{
    public class FormInput
    {
        [Display(Name = "RepairFormTypeDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "RepairFormTypeDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        [DisplayName("納入MTTR計算")]
        public bool MTTR { get; set; }
    }
}
