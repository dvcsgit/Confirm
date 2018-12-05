using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QS.RemarkManagement
{
    public class FormInput
    {
        [DisplayName("備註內容")]
        [Required(ErrorMessage="請輸入備註內容")]
        public string Description { get; set; }
    }
}
