using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.CalibrationApply
{
    public class OwnerFormInput
    {
       [DisplayName("簽核意見")]
        public string Comment { get; set; }
    }
}
