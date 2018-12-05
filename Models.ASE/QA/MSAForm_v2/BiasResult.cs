using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MSAForm_v2
{
    public class BiasResult
    {
        public decimal? TStatistic { get; set; }

        public decimal? SignificantT { get; set; }

        public string Result { get; set; }

        public string ResultDisplay
        {
            get
            {

                if (Result == "P")
                {
                    return "PASS";
                }
                else if (Result == "F")
                {
                    return "FAIL";
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
