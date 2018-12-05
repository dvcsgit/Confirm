using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.InventoryManagerManagement
{
    public class DetailViewModel
    {
        public string OrganizationUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public List<string> ManagerList { get; set; }

        public string Managers
        {
            get
            {
                if (ManagerList != null && ManagerList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var manager in ManagerList)
                    {
                        sb.Append(manager);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public DetailViewModel()
        {
            ManagerList = new List<string>();
        }
    }
}
