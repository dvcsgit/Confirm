using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHIMEI.AIMS2FEM.Models
{
    public class Job
    {
        public string Date { get; set; }

        public string Motor { get; set; }

        public string Cycle { get; set; }

        public string RouteUniqueID { get; set; }

        public List<CHIMEI_Equipment> EquipmentList { get; set; }

        public List<string> Content { get; set; }

        public Job()
        {
            EquipmentList = new List<CHIMEI_Equipment>();
        }
    }
}
