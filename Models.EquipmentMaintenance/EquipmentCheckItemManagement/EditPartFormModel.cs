namespace Models.EquipmentMaintenance.EquipmentCheckItemManagement
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
