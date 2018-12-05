using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASE.HRTrans
{
    public class Organization
    {
        public string UniqueID { get; set; }

        public string ParentUniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public string ManagerID { get; set; }

        public bool HR { get; set; }
    }
}
