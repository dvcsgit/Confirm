namespace Models.PipelinePatrol.CheckPointManagement
{
    public class CheckItemModel
    {
        public bool IsInherit { get; set; }

        public string UniqueID { get; set; }

        public string CheckType { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }

        public bool IsFeelItem { get; set; }
        public bool IsAccumulation { get; set; }

#if ORACLE
        public decimal? OriUpperLimit { get; set; }
        public decimal? OriUpperAlertLimit { get; set; }
        public decimal? OriLowerAlertLimit { get; set; }
        public decimal? OriLowerLimit { get; set; }
#else
        public double? OriUpperLimit { get; set; }
        public double? OriUpperAlertLimit { get; set; }
        public double? OriLowerAlertLimit { get; set; }
        public double? OriLowerLimit { get; set; }
        public double? OriAccumulationBase { get; set; }
#endif

        public string OriUnit { get; set; }
        public string OriRemark { get; set; }

#if ORACLE
        public decimal? UpperLimit { get; set; }
        public decimal? UpperAlertLimit { get; set; }
        public decimal? LowerAlertLimit { get; set; }
        public decimal? LowerLimit { get; set; }
#else
        public double? UpperLimit { get; set; }
        public double? UpperAlertLimit { get; set; }
        public double? LowerAlertLimit { get; set; }
        public double? LowerLimit { get; set; }
        public double? AccumulationBase { get; set; }
#endif

        public string Unit { get; set; }
        public string Remark { get; set; }

        public CheckItemModel()
        {
            IsInherit = true;
        }
    }
}
