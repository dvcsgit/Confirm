using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QS.CheckListManagement
{
    public class CheckTypeModel
    {
        public decimal ID { get; set; }

        public string EDescription { get; set; }

        public string CDescription { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public List<PhotoModel> PhotoList
        {
            get
            {
                return CheckItemList.SelectMany(x => x.PhotoList).ToList();
            }
        }

        public CheckTypeModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
