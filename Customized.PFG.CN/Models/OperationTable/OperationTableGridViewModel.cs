using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.PFG.CN.Models.OperationTable
{
    public class OperationTableGridViewModel
    {

        public string RouteUniqueID { get; set; }
        public string OrganizationDescription { get; set; }
        public OperationTableQueryParameters Parameters { get; set; }

        public List<OperationTableGridItem> ItemList { get; set; }

        public OperationTableGridViewModel()
        {
            Parameters = new OperationTableQueryParameters();
            ItemList = new List<OperationTableGridItem>();
        }
    }
}
