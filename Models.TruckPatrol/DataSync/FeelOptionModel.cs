using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.TruckPatrol.DataSync
{
    public class FeelOptionModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public bool IsAbnormal { get; set; }

        public int Seq { get; set; }
    }
}
