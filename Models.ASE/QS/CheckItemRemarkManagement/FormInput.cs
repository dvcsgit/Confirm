using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models.ASE.QS.CheckItemRemarkManagement
{
    public class FormInput
    {
        public string Remarks { get; set; }

        public List<string> RemarkList
        {
            get
            {
                return JsonConvert.DeserializeObject<List<string>>(Remarks);
            }
        }
    }
}
