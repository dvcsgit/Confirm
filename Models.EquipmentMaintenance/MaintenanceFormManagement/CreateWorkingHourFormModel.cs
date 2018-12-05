using System;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class CreateWorkingHourFormModel
    {
        public WorkingHourFormInput FormInput { get; set; }

        public CreateWorkingHourFormModel()
        {
            FormInput = new WorkingHourFormInput();
        }
    }
}
