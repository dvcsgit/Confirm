using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSAForm_v2
{
    public class StabilityItem
    {
        public int Point { get; set; }

        public DateTime? Date { get; set; }

        public string DateString
        {
            get
            {
                if (Date.HasValue)
                {
                    return Date.Value.ToString("MM/dd");
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public List<StabilityValue> ValueList { get; set; }

        public StabilityItem()
        {
            ValueList = new List<StabilityValue>();
        }
    }
}
