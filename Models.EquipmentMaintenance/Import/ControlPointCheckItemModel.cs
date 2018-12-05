using System.Text;

namespace Models.EquipmentMaintenance.Import
{
    public class ControlPointCheckItemModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public bool IsInherit
        {
            get
            {
                return !(LowerLimit.HasValue || LowerAlertLimit.HasValue || UpperAlertLimit.HasValue || UpperLimit.HasValue || !string.IsNullOrEmpty(Unit) || !string.IsNullOrEmpty(Remark));
            }
        }

        public string LowerLimitString { get; set; }

        public string LowerAlertLimitString { get; set; }

        public string UpperAlertLimitString { get; set; }

        public string UpperLimitString { get; set; }

#if ORACLE
        public decimal? LowerLimit
        {
            get
            {
                if (!string.IsNullOrEmpty(LowerLimitString))
                {
                    decimal lowerLimit;

                    if (decimal.TryParse(LowerLimitString, out lowerLimit))
                    {
                        return lowerLimit;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public decimal? LowerAlertLimit
        {
            get
            {
                if (!string.IsNullOrEmpty(LowerAlertLimitString))
                {
                    decimal lowerAlertLimit;

                    if (decimal.TryParse(LowerAlertLimitString, out lowerAlertLimit))
                    {
                        return lowerAlertLimit;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public decimal? UpperAlertLimit
        {
            get
            {
                if (!string.IsNullOrEmpty(UpperAlertLimitString))
                {
                    decimal upperAlertLimit;

                    if (decimal.TryParse(UpperAlertLimitString, out upperAlertLimit))
                    {
                        return upperAlertLimit;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public decimal? UpperLimit
        {
            get
            {
                if (!string.IsNullOrEmpty(UpperLimitString))
                {
                    decimal upperLimit;

                    if (decimal.TryParse(UpperLimitString, out upperLimit))
                    {
                        return upperLimit;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
#else
         public double? LowerLimit
        {
            get
            {
                if (!string.IsNullOrEmpty(LowerLimitString))
                {
                    double lowerLimit;

                    if (double.TryParse(LowerLimitString, out lowerLimit))
                    {
                        return lowerLimit;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public double? LowerAlertLimit
        {
            get
            {
                if (!string.IsNullOrEmpty(LowerAlertLimitString))
                {
                    double lowerAlertLimit;

                    if (double.TryParse(LowerAlertLimitString, out lowerAlertLimit))
                    {
                        return lowerAlertLimit;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public double? UpperAlertLimit
        {
            get
            {
                if (!string.IsNullOrEmpty(UpperAlertLimitString))
                {
                    double upperAlertLimit;

                    if (double.TryParse(UpperAlertLimitString, out upperAlertLimit))
                    {
                        return upperAlertLimit;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public double? UpperLimit
        {
            get
            {
                if (!string.IsNullOrEmpty(UpperLimitString))
                {
                    double upperLimit;

                    if (double.TryParse(UpperLimitString, out upperLimit))
                    {
                        return upperLimit;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }
#endif

        public string Unit { get; set; }

        public string Remark { get; set; }

        public string Display
        {
            get
            {
                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    return string.Format("{0}({1})", Description, ErrorMessage);
                }
                else
                {
                    return Description;
                }
            }
        }

        public bool IsLowerLimitValid
        {
            get
            {
                bool IsValid = true;

                if (!string.IsNullOrEmpty(LowerLimitString))
                {
                    double lowerLimit;

                    IsValid = double.TryParse(LowerLimitString, out lowerLimit);
                }

                return IsValid;
            }
        }

        public bool IsLowerAlertLimitValid
        {
            get
            {
                bool IsValid = true;

                if (!string.IsNullOrEmpty(LowerAlertLimitString))
                {
                    double lowerAlertLimit;

                    IsValid = double.TryParse(LowerAlertLimitString, out lowerAlertLimit);
                }

                return IsValid;
            }
        }

        public bool IsUpperAlertLimitValid
        {
            get
            {
                bool IsValid = true;

                if (!string.IsNullOrEmpty(UpperAlertLimitString))
                {
                    double upperAlertLimit;

                    IsValid = double.TryParse(UpperAlertLimitString, out upperAlertLimit);
                }

                return IsValid;
            }
        }

        public bool IsUpperLimitValid
        {
            get
            {
                bool IsValid = true;

                if (!string.IsNullOrEmpty(UpperLimitString))
                {
                    double upperLimit;

                    IsValid = double.TryParse(UpperLimitString, out upperLimit);
                }

                return IsValid;
            }
        }

        public bool IsExist { get; set; }

        public bool IsParentError { get; set; }

        public bool IsError
        {
            get
            {
                return 
                    !IsExist || 
                    IsParentError ||
                    !IsLowerLimitValid || 
                    !IsLowerAlertLimitValid || 
                    !IsUpperAlertLimitValid || 
                    !IsUpperLimitValid || 
                    (!string.IsNullOrEmpty(Unit) && Unit.Length > 32) || 
                    (!string.IsNullOrEmpty(Remark) && Remark.Length > 256);
            }
        }

        public string ErrorMessage
        {
            get
            {
                var sb = new StringBuilder();

                if (!IsExist)
                {
                    sb.Append(string.Format("{0} {1} {2}", Resources.Resource.CheckItemID, ID, Resources.Resource.NotExist));
                    sb.Append("、");
                }

                if (!IsLowerLimitValid)
                {
                    sb.Append(string.Format("{0} {1}", Resources.Resource.LowerLimit, Resources.Resource.FormatError));
                    sb.Append("、");
                }

                if (!IsLowerAlertLimitValid)
                {
                    sb.Append(string.Format("{0} {1}", Resources.Resource.LowerAlertLimit, Resources.Resource.FormatError));
                    sb.Append("、");
                }

                if (!IsUpperAlertLimitValid)
                {
                    sb.Append(string.Format("{0} {1}", Resources.Resource.UpperAlertLimit, Resources.Resource.FormatError));
                    sb.Append("、");
                }

                if (!IsUpperLimitValid)
                {
                    sb.Append(string.Format("{0} {1}", Resources.Resource.UpperLimit, Resources.Resource.FormatError));
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(Unit) && Unit.Length > 32)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.Unit, Resources.Resource.Length, 32));
                    sb.Append("、");
                }

                if (!string.IsNullOrEmpty(Remark) && Remark.Length > 256)
                {
                    sb.Append(string.Format("{0} {1} > {2}", Resources.Resource.Remark, Resources.Resource.Length, 256));
                    sb.Append("、");
                }

                if (sb.Length > 0)
                {
                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }
    }
}
