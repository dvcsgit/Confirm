using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QS.CheckItemManagement
{
    public class FormInput
    {
        [DisplayName("稽核類別代號")]
        [Required(ErrorMessage = "請輸入稽核類別代號")]
        public decimal ID { get; set; }

        [DisplayName("稽核類別英文描述")]
        [Required(ErrorMessage = "請輸入稽核類別英文描述")]
        public string EDescription { get; set; }

        [DisplayName("稽核類別中文描述")]
        [Required(ErrorMessage = "請輸入稽核類別中文描述")]
        public string CDescription { get; set; }

        public string CheckItems { get; set; }

        public List<string> CheckItemStringList
        {
            get
            {
                return JsonConvert.DeserializeObject<List<string>>(CheckItems);
            }
        }
    }
}
