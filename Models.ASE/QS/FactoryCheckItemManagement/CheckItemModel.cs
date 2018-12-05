using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QS.FactoryCheckItemManagement
{
    public class CheckItemModel
    {
        public string UniqueID { get; set; }

        public decimal TypeID { get; set; }

        public string TypeEDescription { get; set; }
        public string TypeCDescription { get; set; }

        public decimal ID { get; set; }

        public string EDescription { get; set; }

        public string CDescription { get; set; }

        public decimal CheckTimes { get; set; }

        public string Unit { get; set; }

        public string UnitDisplay
        {
            get
            {
                return string.Format("{0}{1}/次", CheckTimes, Unit);
            }
        }
    }
}
