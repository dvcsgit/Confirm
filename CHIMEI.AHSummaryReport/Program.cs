using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace CHIMEI.AHSummaryReport
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Log("AHSummaryReport Begin...");

            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (ReportHelper generator = new ReportHelper())
            {
                generator.Send();
            }

            Logger.Log("AHSummaryReport Finished...");

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);

            Logger.Seperator();
        }
    }
}
