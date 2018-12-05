using System.Collections.Generic;
namespace Models.EquipmentMaintenance.MaintenanceJobManagement
{
    public class StandardModel
    {
        public bool IsChecked { get; set; }

        public string UniqueID { get; set; }

        public string MaintenanceType { get; set; }

        public string StandardID { get; set; }

        public string StandardDescription { get; set; }

        public List<FeelOptionModel> OptionList { get; set; }

        public StandardModel()
        {
            OptionList = new List<FeelOptionModel>();
        }
    }
}
