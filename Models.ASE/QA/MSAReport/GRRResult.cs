using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MSAReport
{
    public class GRRResult
    {
        public decimal? GRR { get; set; }

        public string GRRDisplay
        {
            get
            {
                if (GRR.HasValue)
                {
                    return string.Format("{0:P2}", GRR.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public decimal? ndc { get; set; }

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
