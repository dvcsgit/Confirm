using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QS.StationManagement
{
    public class FormInput
    {
        public string Type { get; set; }

        [DisplayName("受稽站別描述")]
        [Required(ErrorMessage="請輸入受稽站別描述")]
        public string Description { get; set; }
    }
}
