using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Models
{
    public class UserLimit
    {
        public string OrganizationUniqueID { get; set; }

        public int Users { get; set; }

        public int MobileUsers { get; set; }
    }
}
