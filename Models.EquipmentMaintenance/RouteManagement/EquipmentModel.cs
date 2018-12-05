using System.Collections.Generic;

namespace Models.EquipmentMaintenance.RouteManagement
{
    public class EquipmentModel
    {
        public string EquipmentUniqueID { get; set; }

        public string PartUniqueID { get; set; }

        public string EquipmentID { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string Display
        {
            get
            {
                if (string.IsNullOrEmpty(PartDescription))
                {
                    return string.Format("{0}/{1}", EquipmentID, EquipmentName);
                }
                else
                {
                    return string.Format("{0}/{1}-{2}", EquipmentID, EquipmentName, PartDescription);
                }
            }
        }

        public int Seq { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public EquipmentModel()
        {
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
