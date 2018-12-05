using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Models.ASE.QA.WeeklyReport
{
    public class QueryParameters
    {
        public string BeginYear { get; set; }

        public string BeginWeek { get; set; }

        public DateTime BeginDate
        {
            get
            {
                var begin = new DateTime(int.Parse(BeginYear), 1, 1);

                var beginWeek = int.Parse(BeginWeek);

                DateTimeFormatInfo dateTimeInfo = DateTimeFormatInfo.CurrentInfo;

                var week = dateTimeInfo.Calendar.GetWeekOfYear(begin, dateTimeInfo.CalendarWeekRule, dateTimeInfo.FirstDayOfWeek);

                while (week != beginWeek)
                {
                    begin = begin.AddDays(7);

                    week = dateTimeInfo.Calendar.GetWeekOfYear(begin, dateTimeInfo.CalendarWeekRule, dateTimeInfo.FirstDayOfWeek);
                }

                while (begin.DayOfWeek != DayOfWeek.Thursday)
                {
                    begin = begin.AddDays(1);
                }

                return begin;
            }
        }

        public string EndYear { get; set; }

        public string EndWeek { get; set; }

        public DateTime EndDate
        {
            get
            {
                var end = new DateTime(int.Parse(EndYear), 1, 1);

                var endWeek = int.Parse(EndWeek);

                DateTimeFormatInfo dateTimeInfo = DateTimeFormatInfo.CurrentInfo;

                var week = dateTimeInfo.Calendar.GetWeekOfYear(end, dateTimeInfo.CalendarWeekRule, dateTimeInfo.FirstDayOfWeek);

                while (week != endWeek)
                {
                    end = end.AddDays(7);

                    week = dateTimeInfo.Calendar.GetWeekOfYear(end, dateTimeInfo.CalendarWeekRule, dateTimeInfo.FirstDayOfWeek);
                }

                while (end.DayOfWeek != DayOfWeek.Wednesday)
                {
                    end = end.AddDays(1);
                }

                end = end.AddDays(7);

                return end;
            }
        }

        [DisplayName("校驗編號")]
        public string CalNo { get; set; }

        [Display(Name = "SerialNo", ResourceType = typeof(Resources.Resource))]
        public string SerialNo { get; set; }

        [DisplayName("廠別")]
        public string FactoryUniqueID { get; set; }

        [DisplayName("儀器名稱")]
        public string IchiName { get; set; }

        [Display(Name = "Brand", ResourceType = typeof(Resources.Resource))]
        public string Brand { get; set; }

        [Display(Name = "Model", ResourceType = typeof(Resources.Resource))]
        public string Model { get; set; }

        public QueryParameters()
        {
            BeginYear = DateTime.Today.Year.ToString();
            EndYear = DateTime.Today.Year.ToString();
        }
    }
}
