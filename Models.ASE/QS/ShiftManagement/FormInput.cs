using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QS.ShiftManagement
{
    public class FormInput
    {
        [DisplayName("受稽班別描述")]
        [Required(ErrorMessage="請輸入受稽班別描述")]
        public string Description { get; set; }
    }
}
