namespace Report.EquipmentMaintenance.Models.CheckResultAnalyze
{
    public class AbnormalReasonModel
    {
        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(Remark))
                {
                    return Remark;
                }
                else
                {
                    return Description;
                }
            }
        }

        public string Description { get; set; }

        public string Remark { get; set; }
    }
}
