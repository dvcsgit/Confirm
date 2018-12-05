using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Utility;

namespace Models.EquipmentMaintenance.EquipmentSpecManagement
{
    public class FormInput
    {
        [Display(Name = "EquipmentType", ResourceType = typeof(Resources.Resource))]
        public string EquipmentType { get; set; }
        
        [Display(Name = "EquipmentSpecDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "EquipmentSpecDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }
        
        public string Options { get; set; }

        public List<OptionModel> OptionList
        {
            get
            {
                var optionList = new List<OptionModel>();

                var temp = JsonConvert.DeserializeObject<List<string>>(Options);

                int seq = 1;

                foreach (var t in temp)
                {
                    string[] x = t.Split(Define.Seperators, StringSplitOptions.None);

                    optionList.Add(new OptionModel()
                    {
                        UniqueID = x[0],
                        Description = x[1],
                        Seq = seq
                    });

                    seq++;
                }

                return optionList;
            }
        }
    }
}
