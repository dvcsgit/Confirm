using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Cleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Log("Cleaner Begin...");

            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (CleanHelper helper = new CleanHelper())
            {
                helper.Clean();
            }

            Logger.Log("Cleaner Finished...");

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);

            Logger.Seperator();
        }
    }
}
