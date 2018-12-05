using System.ComponentModel;

namespace Models.PipelinePatrol.ResultQuery
{
    public class CheckItemExcelItem
    {
        public string IsAbnormal { get; set; }

        public string Route { get; set; }

        public string PipePoint { get; set; }

        public string CheckItem { get; set; }

        public string CheckDate { get; set; }

        public string CheckTime { get; set; }

        public string User { get; set; }

        public string Result { get; set; }

        public string LowerLimit { get; set; }

        public string LowerAlertLimit { get; set; }

        public string UpperAlertLimit { get; set; }

        public string UpperLimit { get; set; }

        public string Unit { get; set; }

        public string AbnormalReasons { get; set; }
    }
}
