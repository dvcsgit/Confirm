using System.Collections.Generic;

namespace Models.EquipmentMaintenance.EquipmentCheckItemManagement
{
    public class PartModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public PartModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
