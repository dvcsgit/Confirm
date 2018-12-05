using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TruckPatrol.DataSync
{
    public class TruckModel
    {
        public string UniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string TruckNo { get; set; }

        public string BindingType { get; set; }

        public bool IsCheckBySeq { get; set; }

        public bool IsShowPrevRecord { get; set; }

        public string Remark { get; set; }

        public List<ControlPointModel> ControlPointList { get; set; }

        public string LastModifyTime { get; set; }

        public List<CheckItemModel> CheckItemList
        {
            get
            {
                return ControlPointList.SelectMany(x => x.CheckItemList).Distinct().ToList();
            }
        }

        public TruckModel()
        {
            ControlPointList = new List<ControlPointModel>();
        }
    }
}
