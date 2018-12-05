using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationNotify
{
    public class DetailItem
    {
        public int Seq { get; set; }

        #region 量測特性
        public string CharacteristicUniqueID { get; set; }

        public string CharacteristicDescription { get; set; }

        public string CharacteristicRemark { get; set; }

        public string Characteristic
        {
            get
            {
                if (CharacteristicUniqueID == Define.OTHER)
                {
                    return CharacteristicRemark;
                }
                else
                {
                    return CharacteristicDescription;
                }
            }
        }
        #endregion

        #region 單位
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
        #endregion

        #region 使用範圍下限
        public string LowerUsingRange { get; set; }

        public string LowerUsingRangeUnitUniqueID { get; set; }

        public string LowerUsingRangeUnitDescription { get; set; }

        public string LowerUsingRangeUnitRemark { get; set; }

        public string LowerUsingRangeUnit
        {
            get
            {
                if (!string.IsNullOrEmpty(LowerUsingRangeUnitUniqueID))
                {
                    if (LowerUsingRangeUnitUniqueID == Define.OTHER)
                    {
                        return LowerUsingRangeUnitRemark;
                    }
                    else
                    {
                        return LowerUsingRangeUnitDescription;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        #endregion

        #region 使用範圍上限
        public string UpperUsingRange { get; set; }

        public string UpperUsingRangeUnitUniqueID { get; set; }

        public string UpperUsingRangeUnitDescription { get; set; }

        public string UpperUsingRangeUnitRemark { get; set; }

        public string UpperUsingRangeUnit
        {
            get
            {
                if (!string.IsNullOrEmpty(UpperUsingRangeUnitUniqueID))
                {
                    if (UpperUsingRangeUnitUniqueID == Define.OTHER)
                    {
                        return UpperUsingRangeUnitRemark;
                    }
                    else
                    {
                        return UpperUsingRangeUnitDescription;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        #endregion

        public string UsingRange
        {
            get
            {
                if (!string.IsNullOrEmpty(LowerUsingRange) && !string.IsNullOrEmpty(UpperUsingRange))
                {
                    return string.Format("{0}{1}~{2}{3}", LowerUsingRange, LowerUsingRangeUnit, UpperUsingRange, UpperUsingRangeUnit);
                }
                else if (!string.IsNullOrEmpty(LowerUsingRange) && string.IsNullOrEmpty(UpperUsingRange))
                {
                    return string.Format(">{0}{1}", LowerUsingRange, LowerUsingRangeUnit);
                }
                else if (string.IsNullOrEmpty(LowerUsingRange) && !string.IsNullOrEmpty(UpperUsingRange))
                {
                    return string.Format("<{0}{1}", UpperUsingRange, UpperUsingRangeUnit);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        #region 使用範圍允收
        public string UsingRangeToleranceSymbol { get; set; }

        public string UsingRangeToleranceSymbolDisplay
        {
            get
            {
                if (UsingRangeToleranceSymbol == "1")
                {
                    return "±";
                }
                else if (UsingRangeToleranceSymbol == "2")
                {
                    return "+";
                }
                else if (UsingRangeToleranceSymbol == "3")
                {
                    return "-";
                }
                else if (UsingRangeToleranceSymbol == "4")
                {
                    return ">";
                }
                else if (UsingRangeToleranceSymbol == "5")
                {
                    return "<";
                }
                else if (UsingRangeToleranceSymbol == "6")
                {
                    return "≧";
                }
                else if (UsingRangeToleranceSymbol == "7")
                {
                    return "≦";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string UsingRangeTolerance { get; set; }

        public string UsingRangeToleranceUnitUniqueID { get; set; }

        public string UsingRangeToleranceUnitDescription { get; set; }

        public string UsingRangeToleranceUnitRemark { get; set; }

        public string UsingRangeToleranceUnit
        {
            get
            {
                if (!string.IsNullOrEmpty(UsingRangeToleranceUnitUniqueID))
                {
                    if (UsingRangeToleranceUnitUniqueID == Define.OTHER)
                    {
                        return UsingRangeToleranceUnitRemark;
                    }
                    else if (UsingRangeToleranceUnitUniqueID == "%")
                    {
                        return "%";
                    }
                    else
                    {
                        return UsingRangeToleranceUnitDescription;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string UsingRangeToleranceDisplay
        {
            get
            {
                return string.Format("{0}{1}{2}", UsingRangeToleranceSymbolDisplay, UsingRangeTolerance, UsingRangeToleranceUnit);
            }
        }
        #endregion

        public List<SubDetailItem> ItemList { get; set; }

        public string Items
        {
            get
            {
                if (ItemList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var item in ItemList)
                    {
                        sb.Append(item.Display);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public DetailItem()
        {
            ItemList = new List<SubDetailItem>();
        }
    }
}
