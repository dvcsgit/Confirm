using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QS.DataSync
{
    public class FactoryModel
    {
        public string UniqueID { get; set; }

        public string Description { get; set; }

        public List<string> CheckItemList { get; set; }

        public List<string> StationList { get; set; }

        public FactoryModel()
        {
            CheckItemList = new List<string>();
            StationList = new List<string>();
        }
    }
}
