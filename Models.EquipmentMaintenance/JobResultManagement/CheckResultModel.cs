namespace Models.EquipmentMaintenance.JobResultManagement
{
    public class CheckResultModel
    {
        public string ArriveRecordUniqueID { get; set; }

        public string UniqueID { get; set; }

        public string CheckDate { get; set; }

        public string CheckTime { get; set; }

        public bool IsAbnormal { get; set; }

        public bool IsAlert { get; set; }
    }
}
