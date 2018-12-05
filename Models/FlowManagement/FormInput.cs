using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Utility;

namespace Models.FlowManagement
{
    public class FormInput
    {
        [Display(Name = "FlowDescription", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "FlowDescriptionRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Description { get; set; }

        public List<FormModel> FormList
        {
            get
            {
                var formList = new List<FormModel>();

                if (!string.IsNullOrEmpty(Forms))
                {
                    var formStringList = JsonConvert.DeserializeObject<List<string>>(Forms);

                    foreach (var formString in formStringList)
                    {
                        string[] temp = formString.Split(Define.Seperators, StringSplitOptions.None);

                        formList.Add(new FormModel()
                        {
                            Form = Define.EnumParse<Define.EnumForm>(temp[0]),
                            RepairFormTypeUniqueID = !string.IsNullOrEmpty(temp[1])?temp[1]:"-"
                        });
                    }
                }

                return formList;
            }
        }

        [Display(Name = "FormType", ResourceType = typeof(Resources.Resource))]
        public string Forms { get; set; }
    }
}
