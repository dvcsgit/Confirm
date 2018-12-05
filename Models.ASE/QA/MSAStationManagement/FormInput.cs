using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.MSAStationManagement
{
    public class FormInput
    {
        [DisplayName("站別名稱")]
        [Required(ErrorMessage="請輸入站別名稱")]
        public string Name { get; set; }
    }
}
