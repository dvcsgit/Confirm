using Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class EditFormModel
    {
        public string UniqueID { get; set; }

        public FormViewModel FormViewModel { get; set; }

        public EditFormModel()
        {
            FormViewModel = new FormViewModel();
        }
    }
}
