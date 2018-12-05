using System.Collections.Generic;

namespace Models.PipelinePatrol.RouteManagement
{
    public class CheckPointModel
    {
        public string UniqueID { get; set; }

        public string PipePointType { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public int? MinTimeSpan { get; set; }

        public int Seq { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public CheckPointModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
