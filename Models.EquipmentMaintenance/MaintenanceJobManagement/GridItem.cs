namespace Models.EquipmentMaintenance.MaintenanceJobManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string Description { get; set; }

        public int CycleCount { get; set; }

        public string CycleMode { get; set; }

        public string Cycle
        {
            get
            {
                var cycleMode = string.Empty;

                if (CycleMode == "D")
                {
                    cycleMode = Resources.Resource.Day;
                }
                else if (CycleMode == "W")
                {
                    cycleMode = Resources.Resource.Week;
                }
                else if (CycleMode == "M")
                {
                    cycleMode = Resources.Resource.Month;
                }
                else if (CycleMode == "Y")
                {
                    cycleMode = Resources.Resource.Year;
                }

                return string.Format("{0}{1}{2}", Resources.Resource.Every, CycleCount, cycleMode);
            }
        }
    }
}
