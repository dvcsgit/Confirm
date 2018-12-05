using System.Collections.Generic;
using System.Web.Mvc;
using Utility;
namespace Models.TruckPatrol.TruckManagement
{
    public class QueryFormModel
    {
        public QueryParameters Parameters { get; set; }

        public QueryFormModel()
        {
            Parameters = new QueryParameters();
        }
    }
}
