using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QA.UnitManagement
{
    public class FormInput
    {
        [Display(Name = "Unit", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "UnitRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        public string ToleranceUnits { get; set; }

        public List<string> ToleranceUnitStringList
        {
            get
            {
                if (!string.IsNullOrEmpty(ToleranceUnits))
                {
                    return JsonConvert.DeserializeObject<List<string>>(ToleranceUnits);
                }
                else
                {
                    return new List<string>();
                }
            }
        }
    }
}
