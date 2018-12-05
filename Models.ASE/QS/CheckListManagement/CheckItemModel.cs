using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QS.CheckListManagement
{
    public class CheckItemModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string EDescription { get; set; }

        public string CDescription { get; set; }

        public decimal CheckTimes { get; set; }

        public string Unit { get; set; }

        public string UnitDisplay
        {
            get
            {
                return string.Format("{0}{1}/次", CheckTimes, Unit);
            }
        }

        public List<RemarkModel> RemarkList { get; set; }

        public List<CheckResultModel> CheckResultList { get; set; }

        public List<PhotoModel> PhotoList
        {
            get
            {
                return CheckResultList.SelectMany(x => x.PhotoList).ToList();
            }
        }

        public CheckItemModel()
        {
            RemarkList = new List<RemarkModel>();
            CheckResultList = new List<CheckResultModel>();
        }
    }
}
