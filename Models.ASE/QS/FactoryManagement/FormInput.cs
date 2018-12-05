using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QS.FactoryManagement
{
    public class FormInput
    {
        [DisplayName("受稽廠別描述")]
        [Required(ErrorMessage="請輸入受稽廠別描述")]
        public string Description { get; set; }
    }
}
