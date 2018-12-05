using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.EquipmentManagement
{
    public class FormInput
    {
        [DisplayName("機台編號")]
        public string MachineNo { get; set; }

        [Display(Name = "SerialNo", ResourceType = typeof(Resources.Resource))]
        public string SerialNo { get; set; }

        [Display(Name = "Brand", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "BrandRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Brand { get; set; }

        [Display(Name = "Model", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "ModelRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Model { get; set; }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }
    }
}
