using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Utility;

namespace Models.TruckPatrol.ResultQuery
{
    public class TruckModel
    {
        public string BindingUniqueID { get; set; }

        public string OrganizationUniqueID { get; set; }

        public string OrganizationDescription { get; set; }

        public string FirstTruckUniqueID { get; set; }

        public string FirstTruckNo { get; set; }

        public string SecondTruckUniqueID { get; set; }

        public string SecondTruckNo { get; set; }

        public List<string> CheckDateList { get; set; }

        public string CheckDate
        {
            get
            {
                if (CheckDateList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var checkDate in CheckDateList)
                    {
                        sb.Append(checkDate);
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

        public List<string> CheckUserList { get; set; }

        public string CheckUser
        {
            get
            {
                if (CheckUserList.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var checkUser in CheckUserList)
                    {
                        sb.Append(checkUser);
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

        public List<ControlPointModel> ControlPointList { get; set; }

        public double CheckItemCount
        {
            get
            {
                return ControlPointList.Sum(x => x.CheckItemCount);
            }
        }

        public double CheckedItemCount
        {
            get
            {
                return ControlPointList.Sum(x => x.CheckedItemCount);
            }
        }

        public bool IsComplete
        {
            get
            {
                return CheckItemCount == CheckedItemCount;
            }
        }

        public string CompleteRate
        {
            get
            {
                if (CheckItemCount == 0)
                {
                    return "-";
                }
                else
                {
                    if (CheckedItemCount == 0)
                    {
                        return "0%";
                    }
                    else
                    {
                        if (IsComplete)
                        {
                            return "100%";
                        }
                        else
                        {
                            return (CheckedItemCount / CheckItemCount).ToString("#0.00%");
                        }
                    }
                }
            }
        }

        public string LabelClass
        {
            get
            {
                if (CheckItemCount == 0)
                {
                    return string.Empty;
                }
                else
                {
                    if (CheckedItemCount == 0)
                    {
                        return Define.Label_Color_Red_Class;
                    }
                    else
                    {
                        if (IsComplete)
                        {
                            return Define.Label_Color_Green_Class;
                        }
                        else
                        {
                            return Define.Label_Color_Red_Class;
                        }
                    }
                }
            }
        }

        public string TimeSpan
        {
            get
            {
                var totalSeconds = ControlPointList.Sum(x => x.TotalSeconds);

                if (totalSeconds == 0)
                {
                    return "-";
                }
                else
                {
                    var ts = new TimeSpan(0, 0, totalSeconds);

                    return ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
                }
            }
        }

        public bool HaveAbnormal
        {
            get
            {
                return ControlPointList.Any(x => x.HaveAbnormal);
            }
        }

        public bool HaveAlert
        {
            get
            {
                return ControlPointList.Any(x => x.HaveAlert);
            }
        }

        public TruckModel()
        {
            ControlPointList = new List<ControlPointModel>();
        }
    }
}
