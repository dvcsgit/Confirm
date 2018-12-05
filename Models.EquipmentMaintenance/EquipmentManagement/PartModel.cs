using System.Collections.Generic;

namespace Models.EquipmentMaintenance.EquipmentManagement
{
    public class PartModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public List<MaterialModel> MaterialList { get; set; }

        public PartModel()
        {
            MaterialList = new List<MaterialModel>();
        }
    }
}
