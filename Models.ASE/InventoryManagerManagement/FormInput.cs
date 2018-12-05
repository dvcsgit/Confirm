using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.InventoryManagerManagement
{
    public class FormInput
    {
        [Required(ErrorMessageResourceName = "OrganizationRequired", ErrorMessageResourceType = typeof(Resources.Resource))]
        public string OrganizationUniqueID { get; set; }

        public string Managers { get; set; }

        public List<string> ManagerList
        {
            get
            {
                var managerList = new List<string>();

                if (!string.IsNullOrEmpty(Managers))
                {
                    managerList = JsonConvert.DeserializeObject<List<string>>(Managers);
                }

                return managerList;
            }
        }
    }
}
