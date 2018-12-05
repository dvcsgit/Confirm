namespace Models.EquipmentMaintenance.DataSync
{
    public class HandlingMethodModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public override bool Equals(object Object)
        {
            return Equals(Object as HandlingMethodModel);
        }

        public override int GetHashCode()
        {
            return UniqueID.GetHashCode();
        }

        public bool Equals(HandlingMethodModel Model)
        {
            return UniqueID.Equals(Model.UniqueID);
        }
    }
}
