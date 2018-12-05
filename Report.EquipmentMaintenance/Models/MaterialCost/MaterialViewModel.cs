using System.ComponentModel;
using System.Collections.Generic;

namespace Report.EquipmentMaintenance.Models.MaterialCost
{
    public class MaterialViewModel
    {
        
        public string BeginDateString { get; set; }
        public string EndDateString { get; set; }
        public List<MaterialListModel> MaterialList { get; set; }
        public List<MaterialReportModel> MaterialReport { get; set; }
        public  MaterialViewModel()
        {
            MaterialList = new List<MaterialListModel>();
            MaterialReport = new List<MaterialReportModel>();
        }
    }
}
