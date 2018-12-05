using System.Collections.Generic;
using Utility;

namespace Customized.ECOVE.Models.AbnormalHandlingManagement
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
