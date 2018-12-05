namespace Models.EquipmentMaintenance.DataSync
{
    public class EquipmentSpecModel
    {
        public string EquipmentUniqueID { get; set; }

        public string Spec { get; set; }

        public string Option { get; set; }

        public string Input { get; set; }

        public string Value
        {
            get
            {
                if (!string.IsNullOrEmpty(Option))
                {
                    return Option;
                }
                else
                {
                    return Input;
                }
            }
        }

        public override bool Equals(object Object)
        {
            return Equals(Object as EquipmentSpecModel);
        }

        public override int GetHashCode()
        {
            return EquipmentUniqueID.GetHashCode() + Spec.GetHashCode();
        }

        public bool Equals(EquipmentSpecModel Model)
        {
            return EquipmentUniqueID.Equals(Model.EquipmentUniqueID) && Spec.Equals(Model.Spec);
        }
    }
}
