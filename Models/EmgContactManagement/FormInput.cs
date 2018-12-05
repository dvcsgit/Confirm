using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Models.EmgContactManagement
{
    public class FormInput
    {
        [Display(Name = "UserID", ResourceType = typeof(Resources.Resource))]
        public string UserID { get; set; }

        [Display(Name = "Title", ResourceType = typeof(Resources.Resource))]
        public string Title { get; set; }

        [Display(Name = "Name", ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        public string Tel { get; set; }

        public List<string> TelList
        {
            get
            {
                if (!string.IsNullOrEmpty(Tel))
                {
                    return JsonConvert.DeserializeObject<List<string>>(Tel);
                }
                else
                {
                    return new List<string>();
                }
            }
        }
    }
}
