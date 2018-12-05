using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QS.FactoryCheckItemManagement
{
    public class FormInput
    {
        public string CheckItems { get; set; }

        public List<string> CheckItemList
        {
            get
            {
                if (!string.IsNullOrEmpty(CheckItems))
                {
                    return JsonConvert.DeserializeObject<List<string>>(CheckItems);
                }
                else
                {
                    return new List<string>();
                }
            }
        }
    }
}
