using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.AbnormalNotifyGroup
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
