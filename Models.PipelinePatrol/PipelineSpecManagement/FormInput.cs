using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Utility;

namespace Models.PipelinePatrol.PipelineSpecManagement
{
    public class FormInput
    {
        [Display(Name = "SpecType", ResourceType = typeof(Resources.Resource))]
        public string Type { get; set; }
        
        [Display(Name = "PipelineSpecDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "PipelineSpecDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
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
