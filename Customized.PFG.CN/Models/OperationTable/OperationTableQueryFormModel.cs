using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.CN.Models.OperationTable
{
    public class OperationTableQueryFormModel
    {
        public OperationTableQueryParameters Parameters { get; set; }

        public OperationTableQueryFormModel()
        {
            Parameters = new OperationTableQueryParameters();
        }
    }
}
