using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.MSAIchiManagement
{
    public class FormInput
    {
        [DisplayName("MSA站別")]
        [Required(ErrorMessage = "請選擇MSA站別")]
        public string StationUniqueID { get; set; }

        [DisplayName("儀器代號")]
        [Required(ErrorMessage = "請輸入儀器代號")]
        public string ID { get; set; }

        [DisplayName("儀器名稱")]
        [Required(ErrorMessage = "請輸入儀器名稱")]
        public string Name { get; set; }
    }
}
