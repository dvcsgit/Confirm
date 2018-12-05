namespace Models.EquipmentMaintenance.Import
{
    public class CheckItemRowData
    {
        public string OrganizationID { get; set; }

        public string CheckType { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public bool IsFeelItem { get; set; }

        public string LowerLimit { get; set; }

        public string LowerAlertLimit { get; set; }

        public string UpperAlertLimit { get; set; }

        public string UpperLimit { get; set; }

        public string Unit { get; set; }

        public string Remark { get; set; }

        public string FeelOption { get; set; }

        public bool IsFeelOptionAbnormal { get; set; }

        public string AbnormalReasonID { get; set; }
    }
}
