using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Models.UserManagement
{
    public class FormInput
    {
        [Display(Name = "UserID", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "UserIDRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string ID { get; set; }

        [Display(Name = "UserName", ResourceType = typeof(Resources.Resource))]
        [Required(ErrorMessageResourceName = "UserNameRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = "Title", ResourceType = typeof(Resources.Resource))]
        public string Title { get; set; }

        [Display(Name = "EMail", ResourceType = typeof(Resources.Resource))]
        public string EMail { get; set; }

        [Display(Name = "UID", ResourceType = typeof(Resources.Resource))]
        public string UID { get; set; }

        [Display(Name = "IsMobileUser", ResourceType = typeof(Resources.Resource))]
        public bool IsMobileUser { get; set; }

        [Display(Name = "AuthGroup", ResourceType = typeof(Resources.Resource))]
        public string AuthGroups { get; set; }

        public List<string> AuthGroupList
        {
            get
            {
                var authGroupList = new List<string>();

                if (!string.IsNullOrEmpty(this.AuthGroups))
                {
                    authGroupList = JsonConvert.DeserializeObject<List<string>>(this.AuthGroups);
                }

                return authGroupList;
            }
        }
    }
}
