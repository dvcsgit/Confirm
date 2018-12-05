using System.Linq;
using System.Collections.Generic;

namespace Models.GuardPatrol.DataSync
{
    public class ControlPointModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public bool IsFeelItemDefaultNormal { get; set; }

        public string TagID { get; set; }

        public int? MinTimeSpan { get; set; }

        public string Remark { get; set; }

        public int Seq { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public ControlPointModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
