using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationNotify
{
    public class SubDetailItem
    {
        public int Seq { get; set; }

        public decimal CalibrationPoint { get; set; }

        public string CalibrationPointUnitUniqueID { get; set; }

        public string CalibrationPointUnitDescription { get; set; }

        public string CalibrationPointDisplay
        {
            get
            {
                return string.Format("{0}{1}", CalibrationPoint, CalibrationPointUnitDescription);
            }
        }

        public string ToleranceSymbol { get; set; }

        public string ToleranceSymbolDisplay
        {
            get
            {
                if (ToleranceSymbol == "1")
                {
                    return "±";
                }
                else if (ToleranceSymbol == "2")
                {
                    return "+";
                }
                else if (ToleranceSymbol == "3")
                {
                    return "-";
                }
                else if (ToleranceSymbol == "4")
                {
                    return ">";
                }
                else if (ToleranceSymbol == "5")
                {
                    return "<";
                }
                else if (ToleranceSymbol == "6")
                {
                    return "≧";
                }
                else if (ToleranceSymbol == "7")
                {
                    return "≦";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public decimal Tolerance { get; set; }

        public string ToleranceUnitUniqueID { get; set; }

        public string ToleranceUnitDescription { get; set; }

        public string ToleranceUnitDisplay
        {
            get
            {
                if (!string.IsNullOrEmpty(ToleranceUnitUniqueID))
                {
                    if (ToleranceUnitUniqueID == "%")
                    {
                        return "%";
                    }
                    else
                    {
                        return ToleranceUnitDescription;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string ToleranceDisplay
        {
            get
            {
                return string.Format("{0}{1}{2}", ToleranceSymbolDisplay, Tolerance, ToleranceUnitDisplay);
            }
        }

        public string Display
        {
            get
            {
                return string.Format("{0}({1})", CalibrationPointDisplay, ToleranceDisplay);
            }
        }
    }
}
