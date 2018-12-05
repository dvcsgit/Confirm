using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.PipelinePatrol.ResultQuery
{
    public class RouteModel
    {
        public string UniqueID { get; set; }

        public string ID { get; set; }

        public string Name { get; set; }

        public string Display
        {
            get
            {
                return string.Format("{0}/{1}", ID, Name);
            }
        }

        public List<PipePointModel> PipePointList { get; set; }

        public double CheckItemCount
        {
            get
            {
                return PipePointList.Sum(x => x.CheckItemCount);
            }
        }

        public double CheckedItemCount
        {
            get
            {
                return PipePointList.Sum(x => x.CheckedItemCount);
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

        public int TotalSeconds
        {
            get
            {
                return PipePointList.Sum(x => x.TotalSeconds);
            }
        }

        public string TimeSpan
        {
            get
            {
                if (TotalSeconds == 0)
                {
                    return "-";
                }
                else
                {
                    var ts = new TimeSpan(0, 0, TotalSeconds);

                    return ts.Hours.ToString().PadLeft(2, '0') + ":" + ts.Minutes.ToString().PadLeft(2, '0') + ":" + ts.Seconds.ToString().PadLeft(2, '0');
                }
            }
        }

        public bool HaveAbnormal
        {
            get
            {
                return PipePointList.Any(x => x.HaveAbnormal);
            }
        }

        public bool HaveAlert
        {
            get
            {
                return PipePointList.Any(x => x.HaveAlert);
            }
        }

        public RouteModel()
        {
            PipePointList = new List<PipePointModel>();
        }
    }
}
