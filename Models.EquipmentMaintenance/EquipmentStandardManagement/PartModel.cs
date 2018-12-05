using System.Collections.Generic;

namespace Models.EquipmentMaintenance.EquipmentStandardManagement
{
    public class PartModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public List<StandardModel> StandardList { get; set; }

        public PartModel()
        {
            StandardList = new List<StandardModel>();
        }
    }
}
