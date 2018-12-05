using System.ComponentModel;

namespace Models.PipelinePatrol.ResultQuery
{
    public class RouteExcelItem
    {
        public string IsAbnormal { get; set; }

        public string Route { get; set; }

        public string PipePoint { get; set; }

        public string CompleteRate { get; set; }

        public string TimeSpan { get; set; }

        public string ArriveDate { get; set; }

        public string ArriveTime { get; set; }

        public string User { get; set; }

        public string MinTimeSpan { get; set; }

        public string IsTimeSpanAbnormal { get; set; }

        public string TimeSpanAbnormalReason { get; set; }
    }
}
