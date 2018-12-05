namespace Models.EquipmentMaintenance.RepairFormTypeManagement
{
    public class GridItem
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public bool? MTTR { get; set; }

        public bool CanDelete { get; set; }
    }
}
