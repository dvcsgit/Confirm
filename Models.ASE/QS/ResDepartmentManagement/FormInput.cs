using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QS.ResDepartmentManagement
{
    public class FormInput
    {
        [DisplayName("負責部門描述")]
        [Required(ErrorMessage="請輸入負責部門描述")]
        public string Description { get; set; }
    }
}
