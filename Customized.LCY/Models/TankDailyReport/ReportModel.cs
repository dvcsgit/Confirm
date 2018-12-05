using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Customized.LCY.Models.TankDailyReport
{
    public class ReportModel
    {
        public string CheckDateString { get; set; }

        public DateTime? CheckDate
        {
            get
            {
                return DateTimeHelper.DateString2DateTime(CheckDateString);
            }
        }

        public string CheckTimeBegin { get; set; }

        public string CheckTimeEnd { get; set; }

        public List<TankModel> TankList { get; set; }

        public List<ValveModel> ValveList { get; set; }

        public string PipeStatus
        {
            get
            {
                if (TankList != null)
                {
                    var pipeList = TankList.Where(x => !string.IsNullOrEmpty(x.Pipe) && x.Pipe != "無").Select(x => x.Pipe).Distinct().OrderBy(x => x).ToList();

                    var sb = new StringBuilder();

                    foreach (var pipe in pipeList)
                    {
                        var tankList = TankList.Where(x => x.Pipe == pipe).Select(x => x.ID).Distinct().OrderBy(x => x).ToList();

                        var tmp = string.Format("{0}(", pipe);

                        foreach (var tank in tankList)
                        {
                            tmp += tank + "、";
                        }

                        tmp = tmp.Substring(0, tmp.Length - 1);

                        tmp += ")";

                        sb.AppendLine(tmp);
                    }

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string ExchangeStatus
        {
            get
            {
                if (TankList != null)
                {
                    var tankList = TankList.Where(x => x.HaveExchange && !string.IsNullOrEmpty(x.Content)).Select(x => new { x.ID, x.Content }).Distinct().OrderBy(x => x.ID).ToList();

                    var sb = new StringBuilder();

                    var tmp = string.Empty;

                    var index = 1;

                    foreach(var tank in tankList)
                    {
                        tmp += string.Format("{0}-{1} ", tank.ID, tank.Content);

                        if (index % 5 == 0)
                        {
                            sb.AppendLine(tmp);

                            tmp = string.Empty;
                        }

                        index++;
                    }

                    return sb.ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public ReportModel()
        {
            TankList = new List<TankModel>();
            ValveList = new List<ValveModel>();
        }
    }
}
