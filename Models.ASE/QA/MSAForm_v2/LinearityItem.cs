using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MSAForm_v2
{
    public class LinearityItem
    {
        public int Trials { get; set; }

        public decimal? ReferenceValue { get; set; }

        public List<LinearityValue> ValueList { get; set; }

        public LinearityItem()
        {
            ValueList = new List<LinearityValue>();
        }
    }
}
