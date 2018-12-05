using System.Collections.Generic;

namespace Models.EquipmentMaintenance.MaintenanceJobManagement
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

        public List<StandardModel> StandardList { get; set; }

        public List<MaterialModel> MaterialList { get; set; }

        public EquipmentModel()
        {
            StandardList = new List<StandardModel>();
            MaterialList = new List<MaterialModel>();
        }
    }
}
