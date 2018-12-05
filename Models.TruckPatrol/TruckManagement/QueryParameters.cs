using System;
using System.ComponentModel.DataAnnotations;
using Utility;

namespace Models.TruckPatrol.TruckManagement
{
    public class QueryParameters
    {
        public string OrganizationUniqueID { get; set; }

        public string Keyword { get; set; }
    }
}
