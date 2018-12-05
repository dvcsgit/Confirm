namespace Models.TruckPatrol.UnRFIDReasonManagement
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
