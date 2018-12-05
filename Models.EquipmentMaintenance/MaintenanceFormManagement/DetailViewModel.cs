using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;
using System.Text;
using System;
using System.Linq;
using Models.Shared;

namespace Models.EquipmentMaintenance.MaintenanceFormManagement
{
    public class DetailViewModel
    {
        public string UniqueID { get; set; }

        public FormViewModel FormViewModel { get; set; }

        public DetailViewModel()
        {
            FormViewModel = new FormViewModel();
        }
    }
}
