namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class MaterialModel
    {
        public string UniqueID { get; set; }

        public string ID { get;set; }

        public string Name { get; set; }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    return string.Format("{0}/{1}", ID, Name);
                }
                else
                {
                    return ID;
                }
            }
        }

        public int Quantity { get; set; }

        public int? ChangeQuantity { get; set; }
    }
}
