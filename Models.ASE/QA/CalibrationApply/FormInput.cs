using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CalibrationApply
{
    public class FormInput
    {
        public string FactoryID { get; set; }

        [Display(Name = "IchiType", ResourceType = typeof(Resources.Resource))]
        public string IchiType { get; set; }

        [DisplayName("機台編號")]
        public string MachineNo { get; set; }

        [Display(Name = "IchiName", ResourceType = typeof(Resources.Resource))]
        public string IchiUniqueID { get; set; }

        public string IchiRemark { get; set; }

        public string CharacteristicType { get; set; }

        [Display(Name = "SerialNo", ResourceType = typeof(Resources.Resource))]
        public string SerialNo { get; set; }

        [Display(Name = "Brand", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "BrandRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Brand { get; set; }

        [Display(Name = "Model", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "ModelRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Model { get; set; }

        [DisplayName("Spec")]
        public string Spec { get; set; }

        [Display(Name = "Calibration", ResourceType = typeof(Resources.Resource))]
        public bool CAL { get; set; }

        [Display(Name = "MSA", ResourceType = typeof(Resources.Resource))]
        public bool MSA { get; set; }

        [Display(Name = "EquipmentOwner", ResourceType = typeof(Resources.Resource))]
        public string OwnerID { get; set; }

        [Display(Name = "EquipmentOwnerManager", ResourceType = typeof(Resources.Resource))]
        public string OwnerManagerID { get; set; }

        [Display(Name = "PE", ResourceType = typeof(Resources.Resource))]
        public string PEID { get; set; }

        [Display(Name = "PEManager", ResourceType = typeof(Resources.Resource))]
        public string PEManagerID { get; set; }

        [Display(Name = "Remark", ResourceType = typeof(Resources.Resource))]
        public string Remark { get; set; }

        [DisplayName("校正頻率(月)")]
        public int? CalCycle { get; set; }

        [DisplayName("MSA頻率(月)")]
        public int? MSACycle { get; set; }
    }
}
