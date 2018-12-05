using System.Collections.Generic;

namespace Models.GuardPatrol.RouteManagement
{
    public class ControlPointModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public int? MinTimeSpan { get; set; }

        public int Seq { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public ControlPointModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
