using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.CharacteristicManagement
{
    public class FormInput
    {
        [DisplayName("量性")]
        public string Type { get; set; }

        [DisplayName("名稱")]
        [Required(ErrorMessage="請輸入量測特性名稱")]
        public string Description { get; set; }

        public string Units { get; set; }

        public List<string> UnitList
        {
            get
            {
                if (!string.IsNullOrEmpty(Units))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Units);
                }
                else
                {
                    return new List<string>();
                }
            }
        }
    }
}
