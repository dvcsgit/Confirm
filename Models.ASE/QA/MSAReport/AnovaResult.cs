using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MSAReport
{
    public class AnovaResult
    {
        public decimal? TV { get; set; }

        public string TVDisplay
        {
            get
            {
                if (TV.HasValue)
                {
                    return string.Format("{0:P2}", TV.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public decimal? PT { get; set; }

        public string PTDisplay
        {
            get
            {
                if (PT.HasValue)
                {
                    return string.Format("{0:P2}", PT.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public decimal? NDC { get; set; }

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
