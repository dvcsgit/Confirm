using System.Linq;
using System.Collections.Generic;
using System;
using Utility;

namespace Models.EquipmentMaintenance.TrendQuery_CheckItem
{
    public class ChartViewModel
    {
        public string CheckItemDescription { get; set; }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

        public List<EquipmentModel> EquipmentList { get; set; }

        public List<DateTime> XaxisList
        {
            get
            {
                return EquipmentList.SelectMany(x => x.CheckResultList).Select(x => x.CheckDateTime).Distinct().OrderBy(x => x).ToList();
            }
        }

        public string TimeFormat
        {
            get
            {
                if (TickSize == "day")
                {
                    return "%y-%m-%d";
                }
                else
                {
                    return "%H:%M:%S";
                }
            }
        }

        public int TickUnit
        {
            get
            {
                var d = Max - Min;

                if (TickSize == "day")
                {
                    if (d.TotalDays > 10)
                    {
                        return Convert.ToInt32(d.TotalDays / 10);
                    }
                    else
                    {
                        return 1;
                    }
                }
                else
                {
                    if (d.TotalHours > 10)
                    {
                        return Convert.ToInt32(d.TotalHours / 10);
                    }
                    else
                    {
                        return 1;
                    }
                }
            }
        }

        public string TickSize
        {
            get
            {
                if (Min.Date != Max.Date)
                {
                    return "day";
                }
                else
                {
                    return "hour";
                }
            }
        }

        public DateTime Min
        {
            get
            {
                return XaxisList.First();
            }
        }

        public DateTime Max
        {
            get
            {
                return XaxisList.Last();
            }
        }

        public ChartViewModel()
        {
            EquipmentList = new List<EquipmentModel>();
        }
    }
}
