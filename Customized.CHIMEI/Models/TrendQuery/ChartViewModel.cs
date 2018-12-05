using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customized.CHIMEI.Models.TrendQuery
{
    public class ChartViewModel
    {
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

        public string CheckType { get; set; }

        public string Display
        {
            get
            {
                return string.Format("{0} ({1}：{2} {3}：{4})", Resources.Resource.TrendChart, (!string.IsNullOrEmpty(ControlPointDescription) ? Resources.Resource.ControlPoint : Resources.Resource.Equipment), (!string.IsNullOrEmpty(ControlPointDescription) ? ControlPointDescription : Equipment), Resources.Resource.CheckType , CheckType);
            }
        }

        public List<double> LowerLimit { get; set; }

        public List<double> LowerAlertLimit { get; set; }

        public List<double> UpperAlertLimit { get; set; }

        public List<double> UpperLimit { get; set; }

        public List<CheckItemModel> CheckItemList { get; set; }

        public List<DateTime> XaxisList
        {
            get
            {
                return CheckItemList.SelectMany(x => x.CheckResultList).Select(x => x.CheckDateTime).Distinct().OrderBy(x => x).ToList();
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
            LowerLimit = new List<double>();
            LowerAlertLimit = new List<double>();
            UpperAlertLimit = new List<double>();
            UpperLimit = new List<double>();
            CheckItemList = new List<CheckItemModel>();
        }
    }
}
