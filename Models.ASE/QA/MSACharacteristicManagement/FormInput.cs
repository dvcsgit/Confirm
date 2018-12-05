using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Models.ASE.QA.MSACharacteristicManagement
{
    public class FormInput
    {
        [DisplayName("MSA儀器")]
        [Required(ErrorMessage = "請選擇MSA儀器")]
        public string IchiUniqueID { get; set; }

        [DisplayName("量測特性")]
        [Required(ErrorMessage = "請輸入量測特性")]
        public string Name { get; set; }

        public string Units { get; set; }

        public List<string> UnitList
        {
            get
            {
                if (!string.IsNullOrEmpty(Units))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Units).Distinct().ToList();
                }
                else
                {
                    return new List<string>();
                }
            }
        }
    }
}
