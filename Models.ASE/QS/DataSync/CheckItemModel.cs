using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QS.DataSync
{
    public class CheckItemModel
    {
        public string UniqueID { get; set; }

        public decimal ID { get; set; }

        public string EDescription { get; set; }

        public string CDescription { get; set; }

        public decimal CheckTimes { get; set; }

        public string Unit { get; set; }

        public List<RemarkModel> RemarkList { get; set; }

        public CheckItemModel()
        {
            RemarkList = new List<RemarkModel>();
        }
    }
}
