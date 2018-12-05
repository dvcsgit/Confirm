using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Report.EquipmentMaintenance.Models.UnArriveResultAnalyze
{
  public   class ArriveRecordModel
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

        public string ControlPoint
        {
            get
            {
                if (!string.IsNullOrEmpty(ControlPointDescription))
                {
                    return string.Format("{0}/{1}", ControlPointID, ControlPointDescription);
                }
                else
                {
                    return ControlPointID;
                }
            }
        }

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

        public string ArriveDateTime
        {
            get
            {
                return DateTimeHelper.DateTime2DateStringWithSeperator(ArriveDate);
            }
        }

        public string JobDescription { get; set; }

        public string RouteID { get; set; }

        public string RouteName { get; set; }

        public string ControlPointID { get; set; }

        public string ControlPointDescription { get; set; }

        public DateTime? ArriveDate { get; set; }

        public string UserID { get; set; }

        public string UserName { get; set; }

    }
}
