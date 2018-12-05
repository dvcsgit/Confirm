using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CalibrationNotify
{
    public class FormInput
    {
        [Display(Name = "IchiName", ResourceType = typeof(Resources.Resource))]
        public string IchiUniqueID { get; set; }

        public string IchiRemark { get; set; }

        public string CharacteristicType { get; set; }

        [DisplayName("Spec")]
        public string Spec { get; set; }
    }
}
