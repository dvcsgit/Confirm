using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.DataSync
{
    public class DownloadParameters
    {
        public string MaintenanceFormUniqueID { get; set; }

        public string RepairFormUniqueID { get; set; }

        public string JobUniqueID { get; set; }

        public bool IsExceptChecked { get; set; }
    }
}
