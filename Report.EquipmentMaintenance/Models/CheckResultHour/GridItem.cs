using System;
using System.Linq;
using System.Collections.Generic;

namespace Report.EquipmentMaintenance.Models.CheckResultHour
{
    public class GridItem
    {
        public string Route
        {
            get
            {
                if (!string.IsNullOrEmpty(JobDescription))
                {
                    if (!string.IsNullOrEmpty(RouteName))
                    {
                        return string.Format("{0}/{1}-{2}", RouteID, RouteName, JobDescription);
                    }
                    else
                    {
                        return string.Format("{0}-{1}", RouteID, JobDescription);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(RouteName))
                    {
                        return string.Format("{0}/{1}", RouteID, RouteName);
                    }
                    else
                    {
                        return RouteID;
                    }
                }
            }
        }

        public string RouteID { get; set; }

        public string RouteName { get; set; }

        public string JobDescription { get; set; }

        public string User
        {
            get
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    return string.Format("{0}/{1}", UserID, UserName);
                }
                else
                {
                    return UserID;
                }
            }
        }

        public string UserID { get; set; }

        public string UserName { get; set; }

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

        public List<ControlPointModel> ControlPointList { get; set; }

        public GridItem()
        {
            ControlPointList = new List<ControlPointModel>();
        }
    }
}
