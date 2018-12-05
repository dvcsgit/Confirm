using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace AbnormalSummaryReport.EquipmentMaintenance
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Log("AbnormalSummaryReport Begin...");

            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (ReportHelper generator = new ReportHelper())
            {
                generator.Send();
            }

            Logger.Log("AbnormalSummaryReport Finished...");

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);

            Logger.Seperator();
        }
    }
}
