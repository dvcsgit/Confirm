﻿namespace Models.EquipmentMaintenance.UnRFIDReasonManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public FormInput FormInput { get; set; }

        public EditFormModel()
        {
            FormInput = new FormInput();
        }
    }
}
