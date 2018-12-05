using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.CHIMEI.E.Models
{
    public class DownloadParameters
    {
        public string RepairFormUniqueID { get; set; }

        public string MaintenanceFormUniqueID { get; set; }

        public string JobUniqueID { get; set; }

        public bool IsExceptChecked { get; set; }
    }
}