namespace Models.EquipmentMaintenance.FileManagement
{
    public class CreateFormModel
    {
        public string OrganizationUniqueID { get; set; }

        public string EquipmentUniqueID { get; set; }

        public string PartUniqueID { get; set; }

        public string MaterialUniqueID { get; set; }

        public string FolderUniqueID { get; set; }

        public FormInput FormInput { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
        }
    }
}
