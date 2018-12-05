using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Utility;
using System.Linq;

namespace Report.EquipmentMaintenance.Models.EquipmentRepairForm
{
    public class ExportItem
    {
      
        public int CycleCount { get; set; }

        public string CycleMode { get; set; }

        //[Display(Name = "Cycle", ResourceType = typeof(Resources.Resource))]
        public string Cycle
        {
            get
            {
                return string.Format("{0}{1}", CycleCount, CycleMode);                                       
            }
        }

        public string StandardPartDescription { get; set; }

        public string StandardDescription { get; set; }

        public string State { get; set; }

        public string Manage { get; set; }

    }
}
