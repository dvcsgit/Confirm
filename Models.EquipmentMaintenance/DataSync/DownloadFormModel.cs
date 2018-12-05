using System.Collections.Generic;

namespace Models.EquipmentMaintenance.DataSync
{
    public class DownloadFormModel
    {
        public string CheckDate { get; set; }

        public List<DownloadParameters> Parameters { get; set; }

        public DownloadFormModel()
        {
            Parameters = new List<DownloadParameters>();
        }
    }
}
