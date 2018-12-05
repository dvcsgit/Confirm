using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QS.DataSync
{
    public class CheckTypeModel
    {
        public decimal ID { get; set; }

        public string EDescription { get; set; }

        public string CDescription { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public List<RemarkModel> RemarkList
        {
            get
            {
                return CheckItemList.SelectMany(x => x.RemarkList).Distinct().ToList();
            }
        }

        public CheckTypeModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
