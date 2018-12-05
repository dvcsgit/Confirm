using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TruckPatrol.DataSync
{
    public class OrganizationModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public List<OrganizationModel> DownStreamOrganizationList { get; set; }

        public OrganizationModel()
        {
            DownStreamOrganizationList = new List<OrganizationModel>();
        }
    }
}
