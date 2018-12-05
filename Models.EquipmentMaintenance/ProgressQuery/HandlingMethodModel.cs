namespace Models.EquipmentMaintenance.ProgressQuery
{
    public class HandlingMethodModel
    {
        public string Description { get; set; }

        public string Remark { get; set; }

        public string HandlingMethod
        {
            get
            {
                if (string.IsNullOrEmpty(Remark))
                {
                    return Description;
                }
                else
                {
                    return string.Format("{0}({1})", Description, Remark);
                }
            }
        }
    }
}
