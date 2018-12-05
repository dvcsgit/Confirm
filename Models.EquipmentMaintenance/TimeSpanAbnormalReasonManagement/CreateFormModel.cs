namespace Models.EquipmentMaintenance.TimeSpanAbnormalReasonManagement
{
    public class CreateFormModel
    {
        public FormInput FormInput { get; set; }

        public CreateFormModel()
        {
            FormInput = new FormInput();
        }
    }
}
