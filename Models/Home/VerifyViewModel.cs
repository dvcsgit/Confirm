using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Home
{
    public class VerifyViewModel
    {
        public int Count
        {
            get
            {
                return EquipmentPatrolVerifyItemList.Count + 
                    MaintenanceFormVerifyItemList.Count +
                    MaintenanceFormExtendVerifyItemList.Count + 
                    RepairFormVerifyItemList.Count+
                    RepairFormExtendVerifyItemList.Count;
            }
        }

        public List<EquipmentPatrolVerifyItem> EquipmentPatrolVerifyItemList { get; set; }

        public List<MaintenanceFormVerifyItem> MaintenanceFormVerifyItemList { get; set; }

        public List<MaintenanceFormVerifyItem> MaintenanceFormExtendVerifyItemList { get; set; }

        public List<RepairFormVerifyItem> RepairFormVerifyItemList { get; set; }

        public List<RepairFormVerifyItem> RepairFormExtendVerifyItemList { get; set; }

        public VerifyViewModel()
        {
            EquipmentPatrolVerifyItemList = new List<EquipmentPatrolVerifyItem>();
            MaintenanceFormVerifyItemList = new List<MaintenanceFormVerifyItem>();
            MaintenanceFormExtendVerifyItemList = new List<MaintenanceFormVerifyItem>();
            RepairFormVerifyItemList = new List<RepairFormVerifyItem>();
            RepairFormExtendVerifyItemList = new List<RepairFormVerifyItem>();
        }
    }
}
