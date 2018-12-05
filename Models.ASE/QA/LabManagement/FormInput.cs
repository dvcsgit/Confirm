using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.LabManagement
{
    public class FormInput
    {
        [DisplayName("實驗室名稱")]
        [Required(ErrorMessage = "請輸入實驗室名稱")]
        public string Description { get; set; }
    }
}
