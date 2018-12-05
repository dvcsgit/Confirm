using System;
using Utility;

namespace SQLite2DB.TruckPatrol
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Log("SQLite2DB Begin...");

            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (TransHelper helper = new TransHelper())
            {
                helper.Trans();
            }

            Logger.Log("SQLite2DB Finished...");

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);

            Logger.Seperator();
        }
    }
}
