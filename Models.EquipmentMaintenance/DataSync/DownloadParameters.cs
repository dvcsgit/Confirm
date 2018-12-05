namespace Models.EquipmentMaintenance.DataSync
{
    public class DownloadParameters
    {
        public string RepairFormUniqueID { get; set; }

        public string MaintenanceFormUniqueID { get; set; }

        public string JobUniqueID { get; set; }

        public bool IsExceptChecked { get; set; }
    }
}
