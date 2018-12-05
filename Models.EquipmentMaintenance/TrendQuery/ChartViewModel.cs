using System.Linq;
using System.Collections.Generic;
using System;

namespace Models.EquipmentMaintenance.TrendQuery
{
    public class ChartViewModel
    {
        public string Type { get; set; }

        public string ControlPointDescription { get; set; }

        public string EquipmentName { get; set; }

        public string PartDescription { get; set; }

        public string Equipment
        {
            get
            {
                if (!string.IsNullOrEmpty(PartDescription))
                {
                    return string.Format("{0}-{1}", EquipmentName, PartDescription);
                }
                else
                {
                    return EquipmentName;
                }
            }
        }

        public string CheckItemUniqueID { get; set; }

        public string CheckItemDescription { get; set; }

        public string Display
        {
            get
            {
                return string.Format("{0} ({1}：{2} {3}：{4})", Resources.Resource.TrendChart, (!string.IsNullOrEmpty(ControlPointDescription) ? Resources.Resource.ControlPoint : Resources.Resource.Equipment), (!string.IsNullOrEmpty(ControlPointDescription) ? ControlPointDescription : Equipment), Resources.Resource.CheckItem, CheckItemDescription);
            }
        }

        public double? LowerLimit { get; set; }

        public double? LowerAlertLimit { get; set; }

        public double? UpperAlertLimit { get; set; }

        public double? UpperLimit { get; set; }

        public List<CheckResultModel> CheckResultList { get; set; }

        public List<string> XaxisList
        {
            get
            {
                return CheckResultList.Select(x => x.CheckDateTime).OrderBy(x => x).ToList();
            }
        }

        public int TickUnit
        {
            get
            {
                var begin = DateTime.Parse(Min);
                var end = DateTime.Parse(Max);

                var d = end - begin;

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

        public string TickSize
        {
            get
            {
                if (Min.Substring(0, 10) != Max.Substring(0, 10))
                {
                    return "day";
                }
                else
                {
                    return "hour";
                }
            }
        }

        public string Min
        {
            get
            {
                return XaxisList.First();
            }
        }

        public string Max
        {
            get
            {
                return XaxisList.Last();
            }
        }

        public ChartViewModel()
        {
            CheckResultList = new List<CheckResultModel>();
        }
    }
}
