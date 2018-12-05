using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SQLite2DB.TankPatrol
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
