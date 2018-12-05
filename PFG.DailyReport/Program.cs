using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace PFG.DailyReport
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Log("PFG.DailyReport Begin...");

            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (TransHelper helper = new TransHelper())
            {
                helper.Trans();
            }

            Logger.Log("PFG.DailyReport Finished...");

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);

            Logger.Seperator();
        }
    }
}
