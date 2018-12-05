using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace SQLite2DB.ASE.QA
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Log("SQLite2DB.QA Begin...");

            Logger.SubSeperator();

            DateTime beginTime = DateTime.Now;

            using (TransHelper helper = new TransHelper())
            {
                helper.Trans();
            }

            Logger.Log("SQLite2DB.QA Finished...");

            DateTime finishedTime = DateTime.Now;

            Logger.TimeSpan(beginTime, finishedTime);

            Logger.Seperator();
        }
    }
}
