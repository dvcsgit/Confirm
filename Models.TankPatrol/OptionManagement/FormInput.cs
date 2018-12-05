using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.TankPatrol.OptionManagement
{
    public class FormInput
    {
        [DisplayName("選項類別")]
        [Required(ErrorMessage = "請選擇選項類別")]
        public string Type { get; set; }

        [DisplayName("選項描述")]
        [Required(ErrorMessage = "請輸入選項描述")]
        public string Description { get; set; }
    }
}
