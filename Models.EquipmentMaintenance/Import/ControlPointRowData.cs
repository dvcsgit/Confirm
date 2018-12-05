namespace Models.EquipmentMaintenance.Import
{
    public class ControlPointRowData
    {
        public string OrganizationID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public bool IsFeelItemDefaultNormal { get; set; }

        public string TagID { get; set; }

        public string Remark { get; set; }

        public string CheckItemID { get; set; }

        public string LowerLimit { get; set; }

        public string LowerAlertLimit { get; set; }

        public string UpperAlertLimit { get; set; }

        public string UpperLimit { get; set; }

        public string Unit { get; set; }

        public string CheckItemRemark { get; set; }
    }
}
