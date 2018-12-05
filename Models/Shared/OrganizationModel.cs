using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Shared
{
    public class OrganizationModel
    {
        public string AncestorOrganizationUniqueID { get; set; }

        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public string FullDescription { get; set; }

        public string ManagerID { get; set; }
    }
}
