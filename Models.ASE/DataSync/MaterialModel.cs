using System.Collections.Generic;

namespace Models.ASE.DataSync
{
    public class MaterialModel
    {
        public string EquipmentUniqueID { get; set; }

        public string PartUniqueID { get; set; }

        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public List<MaterialSpecModel> SpecList { get; set; }

        public MaterialModel()
        {
            SpecList = new List<MaterialSpecModel>();
        }

        public override bool Equals(object Object)
        {
            return Equals(Object as MaterialModel);
        }

        public override int GetHashCode()
        {
            return EquipmentUniqueID.GetHashCode() + PartUniqueID.GetHashCode() + UniqueID.GetHashCode();
        }

        public bool Equals(MaterialModel Model)
        {
            return EquipmentUniqueID.Equals(Model.EquipmentUniqueID) && PartUniqueID.Equals(Model.PartUniqueID) && UniqueID.Equals(Model.UniqueID);
        }
    }
}
