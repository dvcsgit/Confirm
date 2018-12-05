namespace Models.EquipmentMaintenance.Import
{
    public class UploadFormModel
    {
        public string InitialMessage { get; set; }

        public FormInput FormInput { get; set; }

        public UploadFormModel()
        {
            FormInput = new FormInput();
        }
    }
}
