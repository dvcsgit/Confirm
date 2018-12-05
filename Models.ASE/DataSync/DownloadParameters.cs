namespace Models.ASE.DataSync
{
    public class DownloadParameters
    {
        public string MaintenanceFormUniqueID { get; set; }

        public string RepairFormUniqueID { get; set; }

        public string JobUniqueID { get; set; }

        public bool IsExceptChecked { get; set; }
    }
}
