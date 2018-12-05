using Models.PipelinePatrol.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.ResultQuery
{
    public class JobModel
    {
        public string UniqueID { get; set; }

        public string CheckDate { get; set; }

        public string ID { get; set; }

        public string Description { get; set; }

        public string Display
        {
            get
            {
                return string.Format("{0}/{1}", ID, Description);
            }
        }

        public List<RouteModel> RouteList { get; set; }

        public List<PipelineViewModel> PipelineList { get; set; }

        public List<UserLocus> UserLocusList { get; set; }

        //public List<Location> UserLocus { get; set; }

        public List<Location> Locus
        {
            get
            {
                return PipelineList.SelectMany(x => x.Locus).ToList();
            }
        }

        public Location Max
        {
            get
            {
                if (Locus.Count > 0)
                {
                    return new Location()
                    {
                        LAT = Locus.Max(x => x.LAT),
                        LNG = Locus.Max(x => x.LNG)
                    };
                }
                else
                {
                    return new Location() { LAT = 0, LNG = 0 };
                }
            }
        }

        public Location Min
        {
            get
            {
                if (Locus.Count > 0)
                {
                    return new Location()
                    {
                        LAT = Locus.Min(x => x.LAT),
                        LNG = Locus.Min(x => x.LNG)
                    };
                }
                else
                {
                    return new Location() { LAT = 0, LNG = 0 };
                }
            }
        }

        public List<PipePointModel> PipePointList
        {
            get
            {
                return RouteList.SelectMany(x => x.PipePointList).ToList();
            }
        }

        public string CheckUser
        {
            get
            {
                var sb = new StringBuilder();

                if (CheckUserList.Count > 0)
                {
                    foreach (var user in CheckUserList)
                    {
                        sb.Append(user);
                        sb.Append("、");
                    }

                    sb.Remove(sb.Length - 1, 1);
                }

                return sb.ToString();
            }
        }

        public List<string> CheckUserList
        {
            get
            {
                return PipePointList.SelectMany(x => x.CheckUserList).Distinct().ToList();
            }
        }

        public double CheckItemCount
        {
            get
            {
                return RouteList.Sum(x => x.CheckItemCount);
            }
        }

        public double CheckedItemCount
        {
            get
            {
                return RouteList.Sum(x => x.CheckedItemCount);
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
                    if (IsComplete)
                    {
                        return Resources.Resource.Completed;
                    }
                    else
                    {
                        return (CheckedItemCount / CheckItemCount).ToString("#0.00%");
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

        public string TimeSpan
        {
            get
            {
                var totalSeconds = RouteList.Sum(x => x.TotalSeconds);

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
                return RouteList.Any(x => x.HaveAbnormal);
            }
        }

        public bool HaveAlert
        {
            get
            {
                return RouteList.Any(x => x.HaveAlert);
            }
        }

        public JobModel()
        {
            RouteList = new List<RouteModel>();
            PipelineList = new List<PipelineViewModel>();
            UserLocusList = new List<UserLocus>();
            //UserLocus = new List<Location>();
        }
    }
}
