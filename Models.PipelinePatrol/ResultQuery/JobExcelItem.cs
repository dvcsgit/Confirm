using System.ComponentModel;

namespace Models.PipelinePatrol.ResultQuery
{
    public class JobExcelItem
    {
        public string IsJobAbnormal { get; set; }

        public string Job { get; set; }

        public string CheckDate { get; set; }

        public string User { get; set; }

        public string JobCompleteRate { get; set; }

        public string JobTimeSpan { get; set; }

        public string IsRouteAbnormal { get; set; }

        public string Route { get; set; }

        public string RouteCompleteRate { get; set; }

        public string RouteTimeSpan { get; set; }
    }
}
