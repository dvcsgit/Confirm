using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MSAReport
{
    public class LinearityResult
    {
        public decimal? ta { get; set; }

        public decimal? tb { get; set; }

        public decimal? t58 { get; set; }

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
                    return "REJECT";
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
