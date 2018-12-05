using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MSAForm_v2
{
    public class CountResult
    {
        public decimal? KappaA { get; set; }
        public decimal? KappaB { get; set; }
        public decimal? KappaC { get; set; }

        public string KappaResult { get; set; }

        public string KappaResultDisplay
        {
            get
            {

                if (KappaResult == "P")
                {
                    return "PASS";
                }
                else if (KappaResult == "F")
                {
                    return "FAIL";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public decimal? CountAEffective { get; set; }
        public decimal? CountAError { get; set; }
        public decimal? CountAAlarm { get; set; }

        public decimal? CountBEffective { get; set; }
        public decimal? CountBError { get; set; }
        public decimal? CountBAlarm { get; set; }

        public decimal? CountCEffective { get; set; }
        public decimal? CountCError { get; set; }
        public decimal? CountCAlarm { get; set; }

        public string Result { get; set; }
    }
}
