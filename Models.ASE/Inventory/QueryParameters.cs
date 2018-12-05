using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.Inventory
{
    public class QueryParameters
    {
        public string OrganizationUniqueID { get; set; }

        public string Keyword { get; set; }
    }
}
