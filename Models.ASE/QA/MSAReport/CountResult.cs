using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ASE.QA.MSAReport
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
        public string CountAEffectiveDisplay
        {
            get
            {
                if (CountAEffective.HasValue)
                {
                    return string.Format("{0:P2}", CountAEffective.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public decimal? CountAError { get; set; }
        public string CountAErrorDisplay
        {
            get
            {
                if (CountAError.HasValue)
                {
                    return string.Format("{0:P2}", CountAError.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public decimal? CountAAlarm { get; set; }
        public string CountAAlarmDisplay
        {
            get
            {
                if (CountAAlarm.HasValue)
                {
                    return string.Format("{0:P2}", CountAAlarm.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public decimal? CountBEffective { get; set; }
        public string CountBEffectiveDisplay
        {
            get
            {
                if (CountBEffective.HasValue)
                {
                    return string.Format("{0:P2}", CountBEffective.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public decimal? CountBError { get; set; }
        public string CountBErrorDisplay
        {
            get
            {
                if (CountBError.HasValue)
                {
                    return string.Format("{0:P2}", CountBError.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public decimal? CountBAlarm { get; set; }
        public string CountBAlarmDisplay
        {
            get
            {
                if (CountBAlarm.HasValue)
                {
                    return string.Format("{0:P2}", CountBAlarm.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public decimal? CountCEffective { get; set; }
        public string CountCEffectiveDisplay
        {
            get
            {
                if (CountCEffective.HasValue)
                {
                    return string.Format("{0:P2}", CountCEffective.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public decimal? CountCError { get; set; }
        public string CountCErrorDisplay
        {
            get
            {
                if (CountCError.HasValue)
                {
                    return string.Format("{0:P2}", CountCError.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public decimal? CountCAlarm { get; set; }
        public string CountCAlarmDisplay
        {
            get
            {
                if (CountBAlarm.HasValue)
                {
                    return string.Format("{0:P2}", CountBAlarm.Value);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

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
