using System.Collections.Generic;

namespace Models.EquipmentMaintenance.RouteManagement
{
    public class ControlPointModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public int? MinTimeSpan { get; set; }

        public int Seq { get; set; }

        public List<EquipmentModel> EquipmentList { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public ControlPointModel()
        {
            EquipmentList = new List<EquipmentModel>();
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
