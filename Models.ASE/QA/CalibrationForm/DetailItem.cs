using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.CalibrationForm
{
    public class DetailItem
    {
        public bool CanEdit { get; set; }

        public int Seq { get; set; }

        public string Characteristic { get; set; }

        public string UsingRange { get; set; }

        public string RangeTolerance { get; set; }

        public string CalibrationPoint { get; set; }

        public decimal? Standard { get; set; }

        public string Diff
        {
            get
            {
                if (ReadingValue.HasValue && Standard.HasValue)
                {
                    if (IsRate)
                    {
                       return ((ReadingValue.Value - Standard.Value) / Standard.Value).ToString("#0.00000%"); 
                    }
                    else
                    {
                        return string.Format("{0}{1}", ((ReadingValue.Value * ToleranceUnitRate) - (Standard.Value * ToleranceUnitRate)), ToleranceUnit);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string Unit { get; set; }

        public string StandardDisplay
        {
            get
            {
                if (Standard.HasValue)
                {
                    return string.Format("{0}{1}", Standard, Unit);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string ToleranceMark { get; set; }

        public string ToleranceMarkDisplay
        {
            get
            {
                if (ToleranceMark == "1")
                {
                    return "±";
                }
                else if (ToleranceMark == "2")
                {
                    return "+";
                }
                else if (ToleranceMark == "3")
                {
                    return "-";
                }
                else if (ToleranceMark == "4")
                {
                    return ">";
                }
                else if (ToleranceMark == "5")
                {
                    return "<";
                }
                else if (ToleranceMark == "6")
                {
                    return "≧";
                }
                else if (ToleranceMark == "7")
                {
                    return "≦";
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public decimal? Tolerance { get; set; }

        public bool IsToleranceInclude
        {
            get
            {
                if (ToleranceMark == "1" || ToleranceMark == "2" || ToleranceMark == "3" || ToleranceMark == "6" || ToleranceMark == "7")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public string ToleranceUnit { get; set; }

        public decimal ToleranceUnitRate { get; set; }

        public string ToleranceDisplay
        {
            get
            {
                return string.Format("{0}{1}{2}", ToleranceMarkDisplay, Tolerance, ToleranceUnit);
            }
        }

        public DateTime? CalibrateDate { get; set; }

        public string CalibrateDateString
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(CalibrateDate);
            }
        }

        public decimal? ReadingValue { get; set; }

        public string ReadingValueDisplay
        {
            get
            {
                if (ReadingValue.HasValue)
                {
                    return string.Format("{0}{1}", ReadingValue, Unit);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public decimal? LowerLimit
        {
            get
            {
                if (Standard.HasValue && Tolerance.HasValue)
                {
                    if (ToleranceMark == "1" || ToleranceMark == "3")
                    {
                        if (IsRate)
                        {
                            var s = (Standard.Value * Tolerance.Value / 100);

                            if (s < 0)
                            {
                                s = s * -1;
                            }

                            return Standard.Value - s;
                        }
                        else
                        {
                            var s = (Tolerance.Value / ToleranceUnitRate);

                            if (s < 0)
                            {
                                s = s * -1;
                            }

                            return Standard.Value - s;
                        }
                    }
                    else if (ToleranceMark == "2")
                    {
                        return Standard.Value;
                    }
                    else if (ToleranceMark == "4" || ToleranceMark == "6")
                    {
                        if (IsRate)
                        {
                            var s = (Standard.Value * Tolerance.Value / 100);

                            if (s < 0)
                            {
                                s = s * -1;
                            }

                            return Standard.Value + s;
                        }
                        else
                        {
                            var s = (Tolerance.Value / ToleranceUnitRate);

                            if (s < 0)
                            {
                                s = s * -1;
                            }

                            return Standard.Value + s;
                        }
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
                if (Standard.HasValue && Tolerance.HasValue)
                {
                    if (ToleranceMark == "1" || ToleranceMark == "2" || ToleranceMark == "5" || ToleranceMark == "7")
                    {
                        if (IsRate)
                        {
                            var s = (Standard.Value * Tolerance.Value / 100);

                            if (s < 0)
                            {
                                s = s * -1;
                            }

                            return Standard.Value + s;
                        }
                        else
                        {
                            var s = (Tolerance.Value / ToleranceUnitRate);

                            if (s < 0)
                            {
                                s = s * -1;
                            }

                            return Standard.Value + s;
                        }
                    }
                    else if (ToleranceMark == "3")
                    {
                        return Standard.Value;
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

        public bool? IsFailed
        {
            get
            {
                if (ReadingValue.HasValue)
                {
                    if (LowerLimit.HasValue)
                    {
                        if (IsToleranceInclude)
                        {
                            if (ReadingValue.Value < LowerLimit.Value)
                            {
                                return true;
                            }
                            //if (Standard.Value < 0)
                            //{
                            //    if (ReadingValue.Value > 0)
                            //    {
                            //        if (ReadingValue.Value > LowerLimit.Value)
                            //        {
                            //            return true;
                            //        }
                            //    }
                            //    else
                            //    {
                            //        if (ReadingValue.Value < LowerLimit.Value)
                            //        {
                            //            return true;
                            //        }
                            //    }
                            //}
                            //else
                            //{

                            //}
                        }
                        else
                        {
                            if (ReadingValue.Value <= LowerLimit.Value)
                            {
                                return true;
                            }
                            //if (Standard.Value < 0)
                            //{
                            //    if (ReadingValue.Value > 0)
                            //    {
                            //        if (ReadingValue.Value >= LowerLimit.Value)
                            //        {
                            //            return true;
                            //        }
                            //    }
                            //    else
                            //    {
                            //        if (ReadingValue.Value <= LowerLimit.Value)
                            //        {
                            //            return true;
                            //        }
                            //    }
                            //}
                            //else
                            //{
                            //    if (ReadingValue.Value <= LowerLimit.Value)
                            //    {
                            //        return true;
                            //    }
                            //}
                        }
                    }

                    if (UpperLimit.HasValue)
                    {
                        if (IsToleranceInclude)
                        {
                            if (ReadingValue.Value > UpperLimit.Value)
                            {
                                return true;
                            }
                            //if (Standard.Value < 0)
                            //{
                            //    if (ReadingValue.Value > 0)
                            //    {
                            //        if (ReadingValue.Value < UpperLimit.Value)
                            //        {
                            //            return true;
                            //        }
                            //    }
                            //    else
                            //    {
                            //        if (ReadingValue.Value > UpperLimit.Value)
                            //        {
                            //            return true;
                            //        }
                            //    }
                            //}
                            //else
                            //{
                            //    if (ReadingValue.Value > UpperLimit.Value)
                            //    {
                            //        return true;
                            //    }
                            //}
                        }
                        else
                        {
                            if (ReadingValue.Value >= UpperLimit.Value)
                            {
                                return true;
                            }
                            //if (Standard.Value < 0)
                            //{
                            //    if (ReadingValue.Value > 0)
                            //    {
                            //        if (ReadingValue.Value <= UpperLimit.Value)
                            //        {
                            //            return true;
                            //        }
                            //    }
                            //    else
                            //    {
                            //        if (ReadingValue.Value >= UpperLimit.Value)
                            //        {
                            //            return true;
                            //        }
                            //    }
                            //}
                            //else
                            //{
                            //    if (ReadingValue.Value >= UpperLimit.Value)
                            //    {
                            //        return true;
                            //    }
                            //}
                        }
                    }

                    return false;
                }
                else
                {
                    return null;
                }
            }
        }

        public string Result
        {
            get
            {
                if (IsFailed.HasValue)
                {
                    if (IsFailed.Value)
                    {
                        return Resources.Resource.Abnormal;
                    }
                    else
                    {
                        return Resources.Resource.Normal;
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool IsRate
        {
            get
            {
                return ToleranceUnit == "%" && ToleranceUnitRate == 0;
            }
        }

        public List<string> PhotoFullPathList { get; set; }

        public List<PhotoModel> PhotoModelList
        {
            get
            {
                var photoList = new List<PhotoModel>();

                foreach (var photo in PhotoList)
                {
                    photoList.Add(new PhotoModel()
                    {
                        FileName = photo
                    });
                }

                return photoList;
            }
        }

        public List<string> PhotoList
        {
            get
            {
                if (PhotoFullPathList != null && PhotoFullPathList.Count > 0)
                {
                    var photoList = new List<string>();

                    foreach (var photo in PhotoFullPathList)
                    {
                        photoList.Add(new FileInfo(photo).Name);
                    }

                    return photoList;
                }
                else
                {
                    return new List<string>();
                }
            }
        }

        public DetailItem()
        {
            PhotoFullPathList = new List<string>();
        }
    }
}
