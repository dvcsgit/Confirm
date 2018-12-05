using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.GuardPatrol.DataSync
{
    public class OrganizationModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public List<OrganizationModel> OrganizationList { get; set; }

        public List<JobItem> JobList { get; set; }

        public OrganizationModel()
        {
            OrganizationList = new List<OrganizationModel>();
            JobList = new List<JobItem>();
        }
    }
}
