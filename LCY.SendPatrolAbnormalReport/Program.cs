using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace LCY.SendPatrolAbnormalReport
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Log("SendPatrolAbnormalReport Begin...");

            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (ReportHelper helper = new ReportHelper())
            {
                helper.Send();
            }

            Logger.Log("SendPatrolAbnormalReport Finished...");

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);

            Logger.Seperator();
        }
    }
}
