using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.MSANotify
{
    public class MSACharacteristicModel
    {
        public int Seq { get; set; }

        public string CharateristicUniqueID { get; set; }

        public string CharateristicName { get; set; }

        public string CharateristicRemark { get; set; }

        public string Charateristic
        {
            get
            {
                if (CharateristicUniqueID == Define.OTHER)
                {
                    return CharateristicRemark;
                }
                else
                {
                    return CharateristicName;
                }
            }
        }

        public string UnitUniqueID { get; set; }

        public string UnitDescription { get; set; }

        public string UnitRemark { get; set; }

        public string Unit
        {
            get
            {
                if (!string.IsNullOrEmpty(UnitUniqueID))
                {
                    if (UnitUniqueID == Define.OTHER)
                    {
                        return UnitRemark;
                    }
                    else
                    {
                        return UnitDescription;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string LowerRange { get; set; }

        public string UpperRange { get; set; }

        public string Range
        {
            get
            {
                if (!string.IsNullOrEmpty(LowerRange) && !string.IsNullOrEmpty(UpperRange))
                {
                    return string.Format("{0}~{1}", LowerRange, UpperRange);
                }
                else if (!string.IsNullOrEmpty(LowerRange) && string.IsNullOrEmpty(UpperRange))
                {
                    return string.Format(">{0}", LowerRange);
                }
                else if (string.IsNullOrEmpty(LowerRange) && !string.IsNullOrEmpty(UpperRange))
                {
                    return string.Format("<{0}", UpperRange);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Display
        {
            get
            {
                return string.Format("{0}({1}{2})", Charateristic, Range, Unit);
            }
        }
    }
}
