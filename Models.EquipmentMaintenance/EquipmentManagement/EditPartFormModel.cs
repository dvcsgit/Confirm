namespace Models.EquipmentMaintenance.EquipmentManagement
{
    public class EditPartFormModel
    {
        public string UniqueID { get; set; }

        public PartFormInput FormInput { get; set; }

        public EditPartFormModel()
        {
            FormInput = new PartFormInput();
        }
    }
}
