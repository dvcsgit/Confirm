using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Shared
{
    public class Organization
    {
        public string UniqueID { get; set; }

        public string ParentUniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }
    }
}
