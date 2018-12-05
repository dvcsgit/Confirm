using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MSAForm_v2
{
    public class GRRAppraiser
    {
        public string Appraiser { get; set; }

        public string UserID { get; set; }

        public List<GRRValue> ValueList { get; set; }

        public bool IsFinished
        {
            get
            {
                return !ValueList.Any(x => !x.Value.HasValue);
            }
        }
        
        public decimal? Average
        {
            get
            {
                if (IsFinished)
                {
                    return ValueList.Average(x => x.Value.Value);
                }
                else
                {
                    return null;
                }
            }
        }

        public GRRAppraiser()
        {
            ValueList = new List<GRRValue>();
        }
    }
}
