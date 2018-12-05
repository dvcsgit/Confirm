using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.IchiManagement
{
    public class FormInput
    {
        [DisplayName("類別")]
        public string Type { get; set; }

        [DisplayName("儀器名稱")]
        [Required(ErrorMessage = "請輸入儀器名稱")]
        public string Name { get; set; }

        public string Characteristics { get; set; }

        public List<string> CharacteristicList
        {
            get
            {
                if (!string.IsNullOrEmpty(Characteristics))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Characteristics);
                }
                else
                {
                    return new List<string>();
                }
            }
        }
    }
}
